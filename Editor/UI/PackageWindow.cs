using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kyowon.Package.UI
{
    public class PackageWindow : EditorWindow
    {

        private const string EDITOR_DOWNLOAD_WINDOW_TITLE = "Kyowon Package Window";
        private const string PROJECT_MANAGER_NAME = "com.kyowon.unityplugins.projectmanager";
        private const string GITHUB_PACKAGE_DOWNLOAD_GUIDE = "Kyowon Project Manager 를 다운로드하세요."; //TODO



        [SerializeField] VisualTreeAsset _asset;

        private PackageList PackageList { get { return rootVisualElement.Q<PackageList>("packageList"); } }
        private PackageDetails PackageDetails { get { return rootVisualElement.Q<PackageDetails>("packageDetails"); } }


        public static void ShowWindow()
        {
            PackageWindow window = GetWindow<PackageWindow>();
            window.titleContent = new GUIContent(EDITOR_DOWNLOAD_WINDOW_TITLE);
            window.minSize = new Vector2(600, 170);
        }


        public void CreateGUI()
        {
            _asset.CloneTree(rootVisualElement);

            PackageList.OnSelected += OnPackageSelected;
            PackageDetails.OnUpdated += OnPackageUpdated;

            PackageList.Reload();
        }
        private void OnDisable()
        {
            if (PackageList != null)
            {
                PackageList.OnSelected -= OnPackageSelected;
            }

        }


        private void OnPackageSelected(KyowonPackageInfo info)
        {
            PackageDetails.SetPackage(info);
        }
        private void OnPackageUpdated(KyowonPackageInfo info)
        {
            PackageList.ItemReload(info);
        }
    }
}
