using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kyowon.Package.UI;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace Kyowon.Package
{
    public static class KyowonPackageManager
    {
        private const string SESSION_STARTED = "KyowonPackageManager_Started";

        private static readonly string _cachePath = Path.Combine(Application.dataPath, "../Library/KyowonPackageCache.json");
        private static readonly string _moduleRootPath = Path.Combine(Application.dataPath, "../Packages");
        private static readonly string _kyowonManifestPath = Path.Combine(_moduleRootPath, "KyowonPackageManifest.json");
        private static readonly string _gitignorePath = Path.Combine(_moduleRootPath, ".gitignore");

        private const string PACKAGE_JSON = "package.json";


        private static bool _focused;
        public static bool IsInitialized { get; private set; } = false;

        private static List<KyowonPackageInfo> _packageInfoList;



        [MenuItem("Kyowon/Package/Open Kyowon Package Manager")]
        public static async void ShowEditorWindow()
        {
            bool hasPermission = await KyowonCertificationManager.HasPackagePermission();
            if (!hasPermission)
                CertificationWindow.ShowWindow();
            else
            {
                if (!IsInitialized) Initialize();
                PackageWindow.ShowWindow();
            }
        }



        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            if (KyowonCertificationManager.HasPermission()) Initialize();
            else CertificationWindow.ShowWindow();

            _focused = true;
            EditorApplication.update += CheckFocus;
        }

        private static async void CheckFocus()
        {
            if (!_focused && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                _focused = true;
                if (!KyowonCertificationManager.HasPermission())
                {
                    CertificationWindow.ShowWindow();
                    return;
                }
                if (!IsInitialized) return;

                await SyncPackageManifest();
            }
            else _focused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
        }

        static async void Initialize(bool force = false)
        {
            await LoadPackageInfo(force);
            await SyncPackageManifest();
            SessionState.SetBool(SESSION_STARTED, true);
            IsInitialized = true;
        }




        public static string GetModuleRootPath()
        {
            return _moduleRootPath;
        }
        public static DateTime? GetCacheTime()
        {
            if (File.Exists(_cachePath)) return File.GetLastWriteTime(_cachePath);
            else return null;
        }


        public static List<KyowonPackageInfo> GetPackageInfos() => _packageInfoList;
        private static async Task<List<KyowonPackageInfo>> LoadPackageInfo(bool force = false)
        {
            if (!SessionState.GetBool(SESSION_STARTED, false)) force = true;
            if (force || _packageInfoList == null || _packageInfoList.Count == 0)
            {
                if (!force && File.Exists(_cachePath))
                {
                    var timeDiff = DateTime.Now - File.GetLastWriteTime(_cachePath);
                    if (timeDiff.TotalDays < 3)
                    {
                        string text = File.ReadAllText(_cachePath);
                        _packageInfoList = JsonConvert.DeserializeObject<List<KyowonPackageInfo>>(text);
                        if (_packageInfoList != null && _packageInfoList.Count > 0) return _packageInfoList;
                    }
                }

                _packageInfoList = await KyowonPackageDownloadManager.GetPackageInfoList();
                File.WriteAllText(_cachePath, JsonConvert.SerializeObject(_packageInfoList, Formatting.Indented));
            }
            return _packageInfoList;
        }

        public static async Task InstallPackage(KyowonPackageInfo package, string version)
        {
            var p = new KyowonPackageProgress(string.Format(KyowonPackageProgress.MSG_INSTALL_PACKAGE, package.Name));

            await InstallPackage_Internal(package, version, p);
            UpdateManifest();

            p.SetComplete();
        }
        private static async Task InstallPackage_Internal(KyowonPackageInfo package, string version, KyowonPackageProgress p)
        {
            string modulePath = Path.Combine(_moduleRootPath, package.Name);
            if (Directory.Exists(modulePath)) Directory.Delete(modulePath, true);

            await KyowonPackageDownloadManager.Download(package, version, p);

            var installedInfoPath = Path.Combine(_moduleRootPath, package.Name, PACKAGE_JSON);
            var installedInfo = JsonConvert.DeserializeObject<KyowonPackageInfo_Installed>(File.ReadAllText(installedInfoPath));
            UpdatePackageInfo(package, installedInfo);
        }
        public static void RemovePackage(KyowonPackageInfo package)
        {
            string modulePath = Path.Combine(_moduleRootPath, package.Name);
            if (Directory.Exists(modulePath)) Directory.Delete(modulePath, true);

            package.Version = null;
            UpdateManifest();
        }

        private static void UpdateManifest()
        {
            Dictionary<string, string> manifestInfo = new();
            string gitignoreContent = "";
            foreach (var info in _packageInfoList)
            {
                if (!string.IsNullOrEmpty(info.Version))
                {
                    manifestInfo.Add(info.Name, info.Version);
                    gitignoreContent += info.Name + "\n";
                }
            }
            File.WriteAllText(_kyowonManifestPath, JsonConvert.SerializeObject(manifestInfo, Formatting.Indented));
            File.WriteAllText(_gitignorePath, gitignoreContent);

            AssetDatabase.Refresh();
            UnityEditor.PackageManager.Client.Resolve();
        }


        private async static Task SyncPackageManifest()
        {
            Dictionary<string, string> manifestInfos = new();

            if (File.Exists(_kyowonManifestPath))
            {
                string text = File.ReadAllText(_kyowonManifestPath);
                manifestInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            }
            foreach (var info in _packageInfoList) info.Version = null;

            bool isChanged = false;

            // 설치된 패키지 목록 가져오기
            DirectoryInfo[] packageFolders = new DirectoryInfo(_moduleRootPath).GetDirectories();

            foreach (DirectoryInfo folder in packageFolders)
            {
                // 1. Manifest 파일에 명시되어 있지 않은데 패키지가 설치되어 있을 때
                if (!manifestInfos.ContainsKey(folder.Name))
                {
                    folder.Delete(true);
                    isChanged = true;
                }
                else
                {
                    // 2. 설치되어 있는 패키지는 정보 업데이트해서 가지고 있기
                    var info = _packageInfoList.Find(i => i.Name == folder.Name);
                    var installedInfoPath = Path.Combine(folder.FullName, PACKAGE_JSON);
                    var installedInfo = JsonConvert.DeserializeObject<KyowonPackageInfo_Installed>(File.ReadAllText(installedInfoPath));
                    UpdatePackageInfo(info, installedInfo);
                }
            }

            KyowonPackageProgress progress = null;

            foreach (var info in manifestInfos)
            {
                var packageInfo = _packageInfoList.Find(i => i.Name == info.Key);

                // 3. Manifest 파일에 명시되어 있는데 패키지 설치가 안되어 있을 때 or Manifest 파일에 명시되어 있는데 버전이 다를 때
                if (!packageInfo.IsInstalled || packageInfo.Version != info.Value)
                {
                    isChanged = true;

                    progress ??= new KyowonPackageProgress(KyowonPackageProgress.MSG_SYNC_PACKAGES);
                    var msg = string.Format(KyowonPackageProgress.MSG_INSTALL_PACKAGE, packageInfo.Name);
                    var p = progress.CreateSubProgress(msg, 0, 0.9f);

                    await InstallPackage_Internal(packageInfo, info.Value, p);

                    p.SetComplete();
                }
            }

            if (isChanged)
            {
                UpdateManifest();
            }
            progress?.SetComplete();
        }

        private static void UpdatePackageInfo(KyowonPackageInfo originalInfo, KyowonPackageInfo_Installed installedInfo)
        {
            originalInfo.Version = installedInfo.Version;
            originalInfo.DisplayName = installedInfo.DisplayName;
        }


        public static void Reload()
        {
            IsInitialized = false;
            _packageInfoList = null;
            Initialize(true);
        }
    }
}