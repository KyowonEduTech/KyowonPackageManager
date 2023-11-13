using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace KyowonPackageManager.Editor
{
    public static class KyowonPackageManager
    {
        private static string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "KyowonPackageManager");
        private static string _fileName = "manifest.json";

        [InitializeOnLoadMethod]
        private static void Start()
        {
            if (!KyowonCertificationManager.HasPackagePermission())
            {
                KyowonEditorWindow.ShowEditorWindow(KyowonEditorWindow.WindowType.Certification);
            }
            else
            {
                KyowonEditorWindow.ShowEditorWindow(KyowonEditorWindow.WindowType.PackageDonwlad);
            }
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

        public static async Task InstallPackage(GitHubPackageDetailInfo packageDetailInfo)
        {
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

        public static bool IsInstalled()
        {
            return true;
        }
    
        private static void SetManifest(string packageName)
        {
            if (File.Exists(Path.Combine(_path, _fileName))){

            }
            else
            {
                string content = "";
                File.WriteAllText(Path.Combine(_path, _fileName), content);
            }
        }

       
    }
}