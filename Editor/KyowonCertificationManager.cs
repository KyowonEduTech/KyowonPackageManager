using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;



namespace Kyowon.Package
{
    public class KyowonCertificationManager
    {

#if UNITY_EDITOR_WIN
        private static string _homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#elif UNITY_EDITOR_OSX
        private static string _homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
        private const string NPM_FILE_NAME = ".upmconfig.toml";
        private const string GITHUB_LINK = "https://npm.pkg.github.com/@kyowonedutech";

        private const string KYOWON_CERTIFICATION_SUCCESS = "Kyowon 인증 되었습니다.";
        private const string KYOWON_CERTIFICATION_FAIL = "Kyowon 인증이 필요합니다.";


        private static string _token;

        public static bool HasPermission()
        {
            if (_token == null)
            {
                var configPath = Path.Combine(_homePath, NPM_FILE_NAME);
                return File.Exists(configPath);                
            }
            return true;
        }


        public static async Task<bool> HasPackagePermission(string token = null)
        {
            string upmConfigPath = Path.Combine(_homePath, NPM_FILE_NAME);

            if (File.Exists(upmConfigPath))
            {
                return true;
            }
            else
            {
                if (token != null)
                {
                    _token = token;
                    bool isRight = await KyowonPackageDownloadManager.CheckToken();
                    if (isRight)
                    {
                        MakeUpmConfigFile(token);
                        Debug.Log(KYOWON_CERTIFICATION_SUCCESS);
                        return true;
                    }
                    else
                    {
                        _token = null;
                    }
                }
                Debug.Log(KYOWON_CERTIFICATION_FAIL);
                return false;
            }
        }
         
        private static void MakeUpmConfigFile(string token)
        {
            string registry = $"[npmAuth.\"{GITHUB_LINK}\"]";
            string tokenString = $"token = \"{token}\"";
            string alwaysAuth = "alwaysAuth = true";
            string npmConfigContent = $"{registry}\n{tokenString}\n{alwaysAuth}";

            File.WriteAllText(Path.Combine(_homePath, NPM_FILE_NAME), npmConfigContent);
        }

        public static void DeleteCertiFile()
        {
            if (File.Exists(Path.Combine(_homePath, NPM_FILE_NAME)))
            {
                File.Delete(Path.Combine(_homePath, NPM_FILE_NAME));
            }
        }

        public static string GetToken()
        {
            if (_token == null)
            {
                string removeString = "token = \"";

                StreamReader streamReader = new StreamReader(Path.Combine(_homePath, NPM_FILE_NAME));
                string token = streamReader.ReadLine();

                while (token != null)
                {
                    if (token.Contains(removeString))
                    {
                        token = token.Remove(token.IndexOf(removeString), removeString.Length);
                        token = token.TrimEnd('\"');
                        break;
                    }
                    token = streamReader.ReadLine();
                }

                streamReader.Close();
                Console.ReadLine();

                _token = token;
            }

            return _token;
        }
    }
}