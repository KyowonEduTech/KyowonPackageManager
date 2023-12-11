using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


namespace KyowonPackageManager.Editor
{
    public class KyowonCertificationManager : MonoBehaviour
    {

#if UNITY_EDITOR_WIN
#elif UNITY_EDITOR_OSX
        private static string _homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
        private const string NPM_FILE_NAME = ".upmconfig.toml";
        private const string GITHUB_LINK = "https://npm.pkg.github.com/@kyowonedutech";

        private const string KYOWON_CERTIFICATION_SUCCESS = "Kyowon 인증 되었습니다.";
        private const string KYOWON_CERTIFICATION_FAIL = "Kyowon 인증이 필요합니다.";


        public static async Task<bool> HasPackagePermission(string token = null)
        {
            string upmConfigPath = Path.Combine(_homePath, NPM_FILE_NAME);

            if (File.Exists(upmConfigPath))
            {
                Debug.Log(KYOWON_CERTIFICATION_SUCCESS);
                return true;
            }
            else
            {
                if(token != null)
                {
                   bool isRight = await GitHubAPI.GetPacskageInfo(null, token) != null;
                   if (isRight)
                   {
                        MakeUpmConfigFile(token);
                        return true;
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
            string removeString = "token = \"";

            StreamReader streamReader = new StreamReader(Path.Combine(_homePath, NPM_FILE_NAME));
            string token = streamReader.ReadLine();

            while(token != null)
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

            return token;
        }
    }
}