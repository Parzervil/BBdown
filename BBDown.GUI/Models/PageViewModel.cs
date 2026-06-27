using BBDown.GUI.ViewModels;

namespace BBDown.GUI.Models
{
    public class PageViewModel : ViewModelBase
    {
        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private int _index;
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _duration = "";
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _aid = "";
        public string Aid
        {
            get => _aid;
            set => SetProperty(ref _aid, value);
        }

        private string _cid = "";
        public string Cid
        {
            get => _cid;
            set => SetProperty(ref _cid, value);
        }
    }
}
