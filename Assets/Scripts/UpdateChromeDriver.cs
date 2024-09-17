using System;
using System.Net;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class UpdateChromeDriver
{
  [Serializable]
  public class DriverInfo
  {
    public DriverVersion[] versions;
  }

  [Serializable]
  public class DriverVersion
  {
    public string version;
    public DriverDownload downloads;
  }

  [Serializable]
  public class DriverDownload
  {
    public DriverPlatform[] chromedriver;
  }

  [Serializable]
  public class DriverPlatform
  {
    public string platform;
    public string url;
  }

  public static string getChromeVersion()
  {
    try
    {
      // コマンドプロンプトからChromeのバージョンを取得するコマンドを実行
      Process process = new Process();
      process.StartInfo.FileName = "cmd.exe";
      process.StartInfo.Arguments = "/C \"wmic datafile where name='C:\\\\Program Files\\\\Google\\\\Chrome\\\\Application\\\\chrome.exe' get Version /value\"";
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.Start();

      // コマンドの出力を読み取る
      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      // 出力からバージョン情報を抽出
      string[] lines = output.Split('\n');
      foreach (var line in lines)
      {
        if (line.StartsWith("Version="))
        {
          return line.Split('=')[1].Trim();
        }
      }

      return null; // バージョン情報が取得できない場合
    }
    catch (Exception e)
    {
      UnityEngine.Debug.LogError("Chromeのバージョンを取得できませんでした: " + e.Message);
      return null;
    }
  }

  private static void KillExistingChromeDriverProcesses()
  {
    try
    {
      // ChromeDriver のプロセスをすべて取得して終了する
      var processes = Process.GetProcessesByName("chromedriver");
      foreach (var process in processes)
      {
        process.Kill();
        process.WaitForExit();
      }
    }
    catch (Exception e)
    {
      UnityEngine.Debug.LogError("ChromeDriverのプロセスを終了できませんでした: " + e.Message);
    }
  }


  public static void updateDriver()
  {
    // 既存の ChromeDriver プロセスを終了
    KillExistingChromeDriverProcesses();

    const string DRIVER_INFO_URL = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";
    const string DRIVER_ZIP = "chromedriver.zip";
    string installPath = Application.streamingAssetsPath; // インストール先のパス

    var chromeVersion = getChromeVersion();

    if (string.IsNullOrEmpty(chromeVersion))
    {
      UnityEngine.Debug.LogError("Chromeのバージョンが取得できませんでした。");
      return;
    }

    // ドライバのパスを設定
    string driverExecutablePath = Path.Combine(installPath, "chromedriver.exe");

    // 既存のドライバが正しいバージョンであるかチェック
    if (File.Exists(driverExecutablePath))
    {
      string existingDriverVersion = GetExistingDriverVersion(driverExecutablePath);
      if (!string.IsNullOrEmpty(existingDriverVersion) && existingDriverVersion.StartsWith(chromeVersion.Split('.')[0]))
      {
        UnityEngine.Debug.Log("既に正しいバージョンのChromeDriverが存在します。ダウンロードをスキップします。");
        return;
      }
    }

    var client = new WebClient();
    string driverInfoJson = client.DownloadString(DRIVER_INFO_URL);
    string driverUrl = getDriverUrl(driverInfoJson, chromeVersion);

    if (driverUrl == null)
    {
      UnityEngine.Debug.LogError("適切なバージョンのChromeDriverが見つかりませんでした。");
      return;
    }

    client.DownloadFile(driverUrl, DRIVER_ZIP);

    ZipArchive archive = ZipFile.OpenRead(DRIVER_ZIP);
    var exeList = archive.Entries.Where(i => i.FullName.EndsWith("chromedriver.exe"));

    if (exeList.Count() != 1)
    {
      UnityEngine.Debug.LogError("ChromeDriverの解凍中に問題が発生しました。");
      return;
    }

    exeList.First().ExtractToFile(driverExecutablePath, true);

    UnityEngine.Debug.Log("ChromeDriverを更新しました。");
  }


  private static string GetExistingDriverVersion(string driverPath)
  {
    try
    {
      // コマンドプロンプトから既存のChromeDriverのバージョンを取得する
      Process process = new Process();
      process.StartInfo.FileName = driverPath;
      process.StartInfo.Arguments = "--version";
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.Start();

      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      // 出力からバージョン情報を抽出
      string version = output.Split(' ')[1];
      return version;
    }
    catch (Exception e)
    {
      UnityEngine.Debug.LogError("既存のChromeDriverのバージョンを取得できませんでした: " + e.Message);
      return null;
    }
  }

  public static string getDriverUrl(string driverInfoJson, string chromeVersion)
  {
    var chromeVersionStripped = string.Join('.', chromeVersion.Split('.').Take(3)) + ".";

    // JSON文字列をDriverInfoクラスに変換
    DriverInfo driverInfo = JsonUtility.FromJson<DriverInfo>(driverInfoJson);

    // バージョン情報を取得
    foreach (var version in driverInfo.versions)
    {
      if (version.version.StartsWith(chromeVersionStripped))
      {
        foreach (var platform in version.downloads.chromedriver)
        {
          if (platform.platform == "win32")
          {
            return platform.url;
          }
        }
      }
    }

    // 適切なバージョンが見つからない場合
    return null;
  }
}
