using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
namespace MediaPlayerWPF
{
    /// <summary>
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        
        
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtEmail.Text.Length == 0)
            {
                errormessage.Text = "Enter an email.";
                txtEmail.Focus();
            }
            else if (!Regex.IsMatch(txtEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = "Enter a valid email.";
                txtEmail.Select(0, txtEmail.Text.Length);
                txtEmail.Focus();
            }
            else
            {
                string email = txtEmail.Text;
                string password = txtPassword.Password;
                OleDbConnection con_stringin = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source =F:\Final project\FilesServer\FilesServer\FileShareDB.accdb");
                con_stringin.Open();
                OleDbCommand cmd = new OleDbCommand("Select*from users where Email='" + email + "'  and password='" + password + "'", con_stringin);
                
               cmd.CommandType = CommandType.Text;
                 OleDbDataAdapter adapter = new OleDbDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    errormessage.Text = "Success";
                    //string username = dataSet.Tables[0].Rows[0]["FirstName"].ToString() + " " + dataSet.Tables[0].Rows[0]["LastName"].ToString();
                    // welcome.TextBlockName.Text = username;//Sending value from one form to another form.  
                    // welcome.Show();
                   // Close();
                }
                else
                {
                    errormessage.Text = "Sorry! Please enter existing emailid/password.";
                }
                con_stringin.Close();
            }
        }
        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            SignUPWindow reg = new SignUPWindow();
            reg.Show();
            Close();
        }
    }
    }

