using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary1;

namespace WpfApp1
{
    //________________________________ПРИВЯЗКА ДАННЫХ ОБЪЕКТА VMBenchmark К ЭЛЕМЕНТАМ УПРАВЛЕНИЯ ПРИЛОЖЕНИЯ___________________________________
    public class ViewData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public VMGrid VMGrid { get; set; }
        public VMBenchmark VMBenchmark { get; set; }
        private bool data_changed = false;

        public ViewData()
        {
            this.VMBenchmark = new VMBenchmark();
            this.VMGrid = new VMGrid();
        }

        public ViewData(VMBenchmark b)
        {
            this.VMBenchmark = b;
            this.VMGrid = new VMGrid();

            VMBenchmark.TimeCollection.CollectionChanged += TimeCollectionChanged;
            VMBenchmark.AccuracyCollection.CollectionChanged += AccuracyCollectionChanged;
        }

        public void AddVMTime(VMGrid NewGrid)
        {
            try
            {
                VMBenchmark.AddVMTime(NewGrid);
                OnPropertyChanged("EP_by_HA_Max");
                OnPropertyChanged("EP_by_HA_Min");
            }
            catch (Exception ex)
            { MessageBox.Show("AN EXCEPTION HAS OCCURRED:\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void AddVMAccuracy(VMGrid NewGrid)
        {
            try
            { VMBenchmark.AddVMAccuracy(NewGrid); }
            catch (Exception ex)
            { MessageBox.Show("AN EXCEPTION HAS OCCURRED:\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public float EP_by_HA_Max
        { get { return VMBenchmark.EP_by_HA_Max; } }

        public float EP_by_HA_Min
        { get { return VMBenchmark.EP_by_HA_Min; } }

        public void Save(string filename)
        {
            BinaryFormatter Formatter = new BinaryFormatter();
            FileStream fs = null;
            try
            {
                using (fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    Formatter.Serialize(fs, VMBenchmark);
                    Console.WriteLine("\nSerialization Complete");
                }
            }
            catch (Exception ex)
            { MessageBox.Show("AN EXCEPTION HAS OCCURRED:\n" + ex.Message); }
            finally
            { if (fs != null) fs.Close(); }
        }

        public void Load(string filename)
        {
            BinaryFormatter Formatter = new BinaryFormatter();
            FileStream fs = null;
            try
            {
                using (fs = new FileStream(filename, FileMode.Open))
                {
                    VMBenchmark LoadedBenchmark = (VMBenchmark)Formatter.Deserialize(fs);
                    for (int i = 0; i < LoadedBenchmark.TimeCollection.Count; i++)
                        this.VMBenchmark.TimeCollection.Add(LoadedBenchmark.TimeCollection[i]);
                    for(int i = 0; i < LoadedBenchmark.AccuracyCollection.Count; i++)
                        this.VMBenchmark.AccuracyCollection.Add(LoadedBenchmark.AccuracyCollection[i]);

                    Console.WriteLine("\nDeserialization Complete");
                }
                OnPropertyChanged("EP_by_HA_Max");
                OnPropertyChanged("EP_by_HA_Min");
            }
            catch (Exception ex)
            { MessageBox.Show("AN EXCEPTION HAS OCCURRED:\n" + ex.Message); }
            finally
            { if (fs != null) fs.Close(); }
        }

        public bool IsChanged
        {
            get
            { return data_changed; }
            set
            { data_changed = value; OnPropertyChanged("IsChanged"); }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        void TimeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        { OnPropertyChanged("VMBenchmark.TimeCollection"); }

        void AccuracyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        { OnPropertyChanged("VMBenchmark.AccuracyCollection"); }
    }

    //______________________________________________________ГЛАВНОЕ ОКНО ПРИЛОЖЕНИЯ WPF_______________________________________________________
    public partial class MainWindow : Window
    {
        public ViewData? ViewData;
        Microsoft.Win32.SaveFileDialog save;
        string name = "Your_file_name_please";
        string ext = ".bin";
        string save_message = "Все несохраненные данные будут потеряны.\nЖелаеете сохранить их?";

        public MainWindow()
        {
            save = new Microsoft.Win32.SaveFileDialog();
            save.FileName = name;
            save.DefaultExt = ext;

            InitializeComponent();
            
            VMBenchmark VMBenchmark = new VMBenchmark();
            ViewData = new ViewData(VMBenchmark);
            
            this.DataContext = ViewData;
            WMfBox.ItemsSource = Enum.GetValues(typeof(VMf));
            TimeCollectionList.ItemsSource = VMBenchmark.TimeCollection;
            AccuracyCollectionList.ItemsSource = VMBenchmark.AccuracyCollection;
        }
        
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (ViewData.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(save_message, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    if (save.ShowDialog() == true)
                    {
                        if (ViewData != null)
                            ViewData.Save(save.FileName);
                    }
                }
                ViewData.IsChanged = false;
            }
            ViewData.VMBenchmark.TimeCollection.Clear();
            ViewData.VMBenchmark.AccuracyCollection.Clear();
        }
        
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (save.ShowDialog() == true)
            {
                if (ViewData != null)
                    ViewData.Save(save.FileName);
            }
            ViewData.IsChanged = false;
        }
        
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (ViewData.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(save_message, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    Microsoft.Win32.SaveFileDialog new_save = new Microsoft.Win32.SaveFileDialog();
                    new_save.FileName = name;
                    new_save.DefaultExt = ".txt";

                    if (new_save.ShowDialog() == true)
                    {
                        if (ViewData != null)
                            ViewData.Save(new_save.FileName);
                    }
                }
                ViewData.IsChanged = false;
            }
            ViewData.VMBenchmark.TimeCollection.Clear();
            ViewData.VMBenchmark.AccuracyCollection.Clear();

            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.FileName = name;
            open.DefaultExt = ".txt";

            if (open.ShowDialog() == true)
                ViewData.Load(open.FileName);
        }
        
        private void AddVMTime_Click(object sender, RoutedEventArgs e)
        {
            ViewData.AddVMTime(ViewData.VMGrid);
            ViewData.IsChanged = true;
        }
        
        private void AddVMAccuracy_Click(object sender, RoutedEventArgs e)
        {
            ViewData.AddVMAccuracy(ViewData.VMGrid);
            ViewData.IsChanged = true;
        }
        
        void WpfApp1_Closing(object sender, CancelEventArgs e)
        {
            if (ViewData.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(save_message, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    if (save.ShowDialog() == true)
                    {
                        if (ViewData != null)
                            ViewData.Save(save.FileName);
                    }
                }
                ViewData.IsChanged = false;
            }
        }
    }

    //______________________________________________________________КОНВЕРТЕРЫ________________________________________________________________
    public class NumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string Num = value.ToString();
                return Num;
            }
            catch
            { return "EX"; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = null;
            try
            {
                string str = value as string;
                int Num;
                Int32.TryParse(str, out Num);

                if (Num == 0)
                { Exception EX = new Exception(); throw (EX); }

                obj = Num;
                return obj;
            }
            catch
            { return obj; }
        }
    }

    public class ScopeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string Min = values[0].ToString();
                string Max = values[1].ToString();
                return Min + ";" + Max;
            }
            catch
            { return "EX"; }
        }
        public object[] ConvertBack(object values, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] obj = new object[2];
            try
            {
                string str_vals = values as string;
                string[] vals = str_vals.Split(';', StringSplitOptions.RemoveEmptyEntries);
                float Min = float.Parse(vals[0]);
                float Max = float.Parse(vals[1]);

                if (Min > Max)
                { Exception EX = new Exception(); throw (EX); }

                obj[0] = Min;
                obj[1] = Max;
                return obj;
            }
            catch
            { return obj; }
        }
    }

    public class FloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                float val = (float)value;
                return val.ToString();
            }
            catch
            { return "EX"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return "Данные были изменены";
                else
                    return "";
            }
            catch
            { return "EX"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}