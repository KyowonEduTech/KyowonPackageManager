using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kyowon.Package.UI
{
    public class PackageItem : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PackageItem> { }


        public static string SelectedClassName = "selected";

        private Label NameLabel { get { return this.Q<Label>("packageName"); } }
        private Label VersionLabel { get { return this.Q<Label>("versionLabel"); } }
        private VisualElement StateIcon { get { return this.Q<VisualElement>("stateIcon"); } }
        private VisualElement Container { get { return this.Q<VisualElement>("mainItem"); } }


        public KyowonPackageInfo Info { get; private set; }
        private string _currentState;
        public event Action<PackageItem> OnSelected = delegate { };


        public PackageItem() : this(null) { }
        public PackageItem(KyowonPackageInfo package)
        {
            var asset = Resources.Load<VisualTreeAsset>("PackageItem");
            asset.CloneTree(this);

            this.AddManipulator(new Clickable(Select));
            SetItem(package);
        }

        private void Select()
        {
            OnSelected(this);
        }

        public void SetSelected(bool value)
        {
            if (value)
                Container.AddToClassList(SelectedClassName);
            else
                Container.RemoveFromClassList(SelectedClassName);
        }

        private void SetItem(KyowonPackageInfo package)
        {
            Info = package;
            OnPackageChanged();            
        }

        public void OnPackageChanged()
        {
            if (Info == null) return;

            NameLabel.text = Info.DisplayName ?? Info.Name;
            string version = Info.Version ?? Info.Latest;
            VersionLabel.text = version.ToString();

            var stateClass = "";
            if (Info.IsInstalled) stateClass = "installed";
            if (Info.HasUpdate) stateClass = "updateavailable";
            
            StateIcon.RemoveFromClassList(_currentState);
            StateIcon.AddToClassList(stateClass);


            UIUtils.SetElementDisplay(VersionLabel, true);

            _currentState = stateClass;
        }

    }
}
