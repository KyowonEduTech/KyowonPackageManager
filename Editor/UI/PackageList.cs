using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Threading.Tasks;

namespace Kyowon.Package.UI
{
    public class PackageList : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PackageList> { }


        private const string PROJECT_MANAGER = "com.kyowon.unityplugins.projectmanager";
        private const string LAST_UPDATE_LABEL = "Last updated {0}";


        private ScrollView List { get { return this.Q<ScrollView>("scrollView"); } }
        private VisualElement Empty { get { return this.Q<VisualElement>("emptyArea"); } }
        private Label UpdateLabel { get { return this.Q<Label>("statusLabel"); } }
        private Button RefreshButton { get { return this.Q<Button>("refreshButton"); } }


        private PackageItem _selected;
        public event Action<KyowonPackageInfo> OnSelected = delegate { };


        public PackageList()
        {   
            var asset = Resources.Load<VisualTreeAsset>("PackageList");
            asset.CloneTree(this);
            this.StretchToParentSize();

            UIUtils.SetElementDisplay(Empty, false);
            UIUtils.SetElementDisplay(RefreshButton, false);

            RefreshButton.clickable.clicked += RefreshPackageList;
        }

        public async void Reload()
        {
            while (!KyowonPackageManager.IsInitialized) await Task.Yield();

            SetPackages();

            UIUtils.SetElementDisplay(RefreshButton, true);
            var time = KyowonPackageManager.GetCacheTime();
            if (time != null) UpdateLabel.text = string.Format(LAST_UPDATE_LABEL, time);
        }
        public void ItemReload(KyowonPackageInfo info)
        {
            foreach (var i in List.Children())
            {
                var item = i as PackageItem;
                if (item.Info == info)
                {
                    item.OnPackageChanged();
                    break;
                }
            }
        }

        private void SetPackages()
        {
            List.Clear();

            PackageItem manager = null;
            var infos = KyowonPackageManager.GetPackageInfos();
            foreach (var p in infos)
            {
                var item = AddPackage(p);
                List.Add(item);
                if (p.Name == PROJECT_MANAGER) manager = item;
            }

            if (_selected == null) Select(manager);
        }
        private PackageItem AddPackage(KyowonPackageInfo package)
        {
            var packageItem = new PackageItem(package);
            packageItem.OnSelected += Select;
            return packageItem;
        }

        private void RefreshPackageList()
        {
            KyowonPackageManager.Reload();
            Reload();
        }

        private void Select(PackageItem item)
        {
            if (_selected == item) return;
            _selected?.SetSelected(false);

            _selected = item;
            if (_selected == null) 
            {
                OnSelected(null);
                return;
            }

            _selected.SetSelected(true);
            OnSelected(item.Info);
        }

    }
}
