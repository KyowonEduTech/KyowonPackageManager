
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Device;


namespace KyowonPackageManager.Editor
{
    public static class KyowonPackageManager
    {
        private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "KyowonPackageManager");
        private const string MANIFEST_FILE_NAME = "manifest.json";

        public static async void Start()
        {
            if (KyowonEditorWindow.IsOpenedWindow) return;

            bool hasPermission = await KyowonCertificationManager.HasPackagePermission();
            if (!hasPermission) KyowonEditorWindow.ShowCertificationWindow();
            else KyowonEditorWindow.ShowDownloadWindow();
        }

        public static async Task<List<GitHubPackageInfo>> GetPackageInfo()
        {
            string packageInfoText = await GitHubAPI.GetPacskageInfo();
            return JsonConvert.DeserializeObject<List<GitHubPackageInfo>>(packageInfoText); 
        }

        public static async Task<GitHubPackageDetailInfo> GetPackageDetailInfo(string packageName)
        {
            string packageDetailInfoText = await GitHubAPI.GetPacskageInfo(packageName);
            return JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);
        }

        public static async Task InstallPackage(string packageName)
        {
            GitHubPackageDetailInfo packageDetailInfo = await GetPackageDetailInfo(packageName);
            await GitHubAPI.DownloadPackage(packageDetailInfo);
        }

        public static void RemovePackage()
        {
            //패키지 있는지 확인
            //다운로드 관리 파일에 패키지이름/버전 추가
            //파일 지우기
        }

        public static void UpdatePackage()
        {
            //파일 덮어쓰기
        }

        public static bool IsInstalled(string packageName)
        {
            string packageFolderPath = Path.Combine(Application.dataPath, "KyowonModules");
            string searchFilePath = Path.Combine(packageFolderPath, packageName);

            return Directory.Exists(searchFilePath) ? true : false;
        }

        //download 된 pakcage 관리 파일 생성
        private static void SetManifest(string packageName)
        {
            if (File.Exists(Path.Combine(_path, MANIFEST_FILE_NAME)))
            {

            }
            else
            {
                File.WriteAllText(Path.Combine(_path, MANIFEST_FILE_NAME), null);
            }
        }
    }
}