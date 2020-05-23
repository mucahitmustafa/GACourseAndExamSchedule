using GACourseAndExamSchedule.Algorithm;
using GACourseAndExamSchedule.Data.Reader;
using GACourseAndExamSchedule.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;

namespace GACourseAndExamSchedule
{
    #region Classes

    public partial class MainWindow : Window
    {
        #region Constants and Fields

        private string PARAMETER_CROSSOVER_PROBABILITY = ConfigurationManager.AppSettings.Get("algorithm.crossoverProbability");
        private string PARAMETER_NUMBER_OF_CROSSOVER_POINTS = ConfigurationManager.AppSettings.Get("algorithm.numberOfCrossoverPoints");
        private string PARAMETER_MUTAITION_PROBABILITY = ConfigurationManager.AppSettings.Get("algorithm.mutationProbability");
        private string PARAMETER_MUTAITION_SIZE = ConfigurationManager.AppSettings.Get("algorithm.mutationSize");
        private string PARAMETER_NUMBER_OF_CHROMOSOMES = ConfigurationManager.AppSettings.Get("algorithm.numberOfChromosomes");
        private string PATH_EXCEL_INPUT_DATA = ConfigurationManager.AppSettings.Get("data.input.location");
        private string PATH_EXCEL_OUTPUT_DATA = ConfigurationManager.AppSettings.Get("data.output.location");

        RoomReader _roomReader = new RoomReader();
        CourseReader _courseReader = new CourseReader();
        PrelectorReader _prelectorReader = new PrelectorReader();
        CourseClassReader _courseClassReader = new CourseClassReader();
        StudentGroupReader _studentGroupReader = new StudentGroupReader();

        List<Room> _rooms;
        List<Course> _courses;
        List<Prelector> _prelectors;
        List<CourseClass> _courseClasses;
        List<StudentGroup> _studentGroups;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(PATH_EXCEL_INPUT_DATA) || string.IsNullOrEmpty(PATH_EXCEL_OUTPUT_DATA))
            {
                ShowConfigData();
            } else if (string.IsNullOrEmpty(PARAMETER_CROSSOVER_PROBABILITY)  || string.IsNullOrEmpty(PARAMETER_MUTAITION_PROBABILITY) ||
                        string.IsNullOrEmpty(PARAMETER_MUTAITION_SIZE) || string.IsNullOrEmpty(PARAMETER_NUMBER_OF_CHROMOSOMES) || string.IsNullOrEmpty(PARAMETER_NUMBER_OF_CROSSOVER_POINTS))
            {
                ShowConfigGA();
            } else
            {
                CollectData();
                ShowDashboard();
            }
        }

        #endregion

        #region Private Methods

        private void CollectData()
        {
            _roomReader.ResetData();
            _courseReader.ResetData();
            _prelectorReader.ResetData();
            _courseClassReader.ResetData();
            _studentGroupReader.ResetData();

            try
            {
                _rooms = _roomReader.GetRooms();
            } catch(Exception _ex)
            {
                MessageBox.Show("Sınıf verileri okunurken hata oluştu!", "Veri Okuma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _courses = _courseReader.GetCourses();
            }
            catch (Exception _ex)
            {
                MessageBox.Show("Ders verileri okunurken hata oluştu!", "Veri Okuma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                _prelectors = _prelectorReader.GetPrelectors();
            }
            catch (Exception _ex)
            {
                MessageBox.Show("Öğretim görevlisi verileri okunurken hata oluştu!", "Veri Okuma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                _courseClasses = _courseClassReader.GetCourseClasses();
            }
            catch (Exception _ex)
            {
                MessageBox.Show("Ders oturumu verileri okunurken hata oluştu!", "Veri Okuma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                _studentGroups = _studentGroupReader.GetStudentGroups();
            }
            catch (Exception _ex)
            {
                MessageBox.Show("Öğrenci grubu verileri okunurken hata oluştu!", "Veri Okuma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            lbl_count_room.Content = _rooms == null ? 0 : _rooms.Count;
            lbl_count_course.Content = _courses == null ? 0 : _courses.Count;
            lbl_count_prelector.Content = _prelectors == null ? 0 : _prelectors.Count;
            lbl_count_group.Content = _studentGroups == null ? 0 : _studentGroups.Count;

            int _courseClassesCount = 0;
            List<string> _calculatedCcs = new List<string>();
            foreach (CourseClass cc in _courseClasses)
            {
                string _ccKey = $"{cc.Course.ID}-{cc.Prelector.ID}-{cc.StudentGroups.Count}-{cc.StudentGroups[0].ID}";
                if (!_calculatedCcs.Contains(_ccKey))
                {
                    _calculatedCcs.Add(_ccKey);
                    _courseClassesCount++;
                }
            }

            lbl_count_courseClass.Content = _courseClassesCount;
        }

        private void HideAllPages()
        {
            pnl_configData.Visibility = Visibility.Hidden;
            pnl_configGA.Visibility = Visibility.Hidden;
            pnl_dashboard.Visibility = Visibility.Hidden;
        }

        private void ShowConfigGA()
        {
            txt_mutationSize.Text = PARAMETER_MUTAITION_SIZE;
            txt_mutationProbability.Text = PARAMETER_MUTAITION_PROBABILITY;
            txt_numberOfChromosomes.Text = PARAMETER_NUMBER_OF_CHROMOSOMES;
            txt_crossoverProbability.Text = PARAMETER_CROSSOVER_PROBABILITY;
            txt_numberOfCrossoverPoints.Text = PARAMETER_NUMBER_OF_CROSSOVER_POINTS;

            HideAllPages();
            pnl_configGA.Visibility = Visibility.Visible;
        }

        private void ShowConfigData()
        {
            txt_inputLoc.Text = PATH_EXCEL_INPUT_DATA;
            txt_outputLoc.Text = PATH_EXCEL_OUTPUT_DATA;

            HideAllPages();
            pnl_configData.Visibility = Visibility.Visible;
        }

        private void ShowDashboard()
        {
            HideAllPages();
            pnl_dashboard.Visibility = Visibility.Visible;
        }

        private void UpdateConfigKey(string strKey, string newValue)
        {
            System.Configuration.Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            oConfig.AppSettings.Settings[strKey].Value = newValue;
            oConfig.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion

        #region Event Handlers

        private void btn_ready_Click(object sender, RoutedEventArgs e)
        {
            if (_rooms != null && _courses != null && _prelectors != null && _studentGroups != null && _courseClasses != null &&
                _rooms.Count > 0 && _courses.Count > 0 && _prelectors.Count > 0 && _studentGroups.Count > 0 && _courseClasses.Count > 0)
            {
                ResultForm resultForm = new ResultForm();
                resultForm.Show();
            } else
            {
                MessageBox.Show("Verilerin bir veya birkaçı hazır değil veya boş.", "Veri Hazır Değil!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btn_dataConfig_Click(object sender, RoutedEventArgs e)
        {
            ShowConfigData();
        }

        private void btn_gaConfig_Click(object sender, RoutedEventArgs e)
        {
            ShowConfigGA();
        }

        private void btn_dashboard_Click(object sender, RoutedEventArgs e)
        {
                ShowDashboard();
        }

        private void btn_refreshData_Click(object sender, RoutedEventArgs e)
        {
            _roomReader = new RoomReader();
            _courseReader = new CourseReader();
            _prelectorReader = new PrelectorReader();
            _courseClassReader = new CourseClassReader();
            _studentGroupReader = new StudentGroupReader();

            CollectData();
        }

        private void btn_saveGaConf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txt_crossoverProbability.Text) && (int.Parse(txt_crossoverProbability.Text) != 0) &&
                    !string.IsNullOrEmpty(txt_mutationProbability.Text) && (int.Parse(txt_mutationProbability.Text) != 0) &&
                    !string.IsNullOrEmpty(txt_mutationSize.Text) && (int.Parse(txt_mutationSize.Text) != 0) &&
                    !string.IsNullOrEmpty(txt_numberOfChromosomes.Text) && (int.Parse(txt_numberOfChromosomes.Text) != 0) &&
                    !string.IsNullOrEmpty(txt_numberOfCrossoverPoints.Text) && (int.Parse(txt_numberOfCrossoverPoints.Text) != 0)) {

                    UpdateConfigKey("algorithm.crossoverProbability", txt_crossoverProbability.Text);
                    UpdateConfigKey("algorithm.numberOfCrossoverPoints", txt_numberOfCrossoverPoints.Text);
                    UpdateConfigKey("algorithm.mutationProbability", txt_mutationProbability.Text);
                    UpdateConfigKey("algorithm.mutationSize", txt_mutationSize.Text);
                    UpdateConfigKey("algorithm.numberOfChromosomes", txt_numberOfChromosomes.Text);

                    PARAMETER_CROSSOVER_PROBABILITY = txt_crossoverProbability.Text;
                    PARAMETER_NUMBER_OF_CROSSOVER_POINTS = txt_numberOfCrossoverPoints.Text;
                    PARAMETER_MUTAITION_PROBABILITY = txt_mutationProbability.Text;
                    PARAMETER_MUTAITION_SIZE = txt_mutationSize.Text;
                    PARAMETER_NUMBER_OF_CHROMOSOMES = txt_numberOfChromosomes.Text;

                    MessageBox.Show("Konfigürasyon parametreleri kaydedildi.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowDashboard();
                } else
                {
                    MessageBox.Show("Tüm bilgiler doldurulmalı!", "Veri Girişi Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            } catch
            {
                MessageBox.Show("Tüm parametreler doldurulmalı ve sayı türünde olmalı!", "Veri Girişi Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btn_saveDataConf_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_inputLoc.Text) && !string.IsNullOrEmpty(txt_outputLoc.Text))
            {
                UpdateConfigKey("data.input.location", txt_inputLoc.Text);
                UpdateConfigKey("data.output.location", txt_outputLoc.Text);

                PATH_EXCEL_INPUT_DATA = txt_inputLoc.Text;
                PATH_EXCEL_OUTPUT_DATA = txt_outputLoc.Text;

                MessageBox.Show("Konfigürasyon parametreleri kaydedildi.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowDashboard();
            } else
            {
                MessageBox.Show("Tüm bilgiler doldurulmalı!", "Veri Girişi Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void btn_selectInputLoc_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txt_inputLoc.Text = dialog.SelectedPath;
                }
            }
        }

        private void btn_selectOutputLoc_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txt_outputLoc.Text = dialog.SelectedPath;
                }
            }
        }

        #endregion
    }

    #endregion
}
