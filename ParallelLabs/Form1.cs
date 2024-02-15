using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ParallelLabs
{
    public partial class Form1 : Form 
    {

        private Socket subSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Socket connectionSocket1;
        private Socket subSocket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Socket connectionSocket2;
        private Semaphore semaphore;

        private Image leftImage;
        private Image rightImage;
        private Graphics leftGraphics;
        private Graphics rightGraphics;
        private bool firstTick = true;

        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private Process subProcess2;
        private Process subProcess1;
        byte[] buffer = new byte[4];

        public Form1()
        {
            
            InitializeComponent();
            DialogResult dialogRes = MessageBox.Show("Показать консольный вывод подпроцессов?","Выберите действие",MessageBoxButtons.YesNo);
            bool showConsole = dialogRes == DialogResult.Yes;
            /*Настравиваем таймер*/
            timer.Tick +=tick;
            timer.Interval = 100;
            /*Добавляем обработчик закрытия формы, чтобы убить подпроцессы*/
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            /*Пытаемся создать семафор*/
            try
            {
                semaphore = new Semaphore(0, 1, "Global\\GraphicsSemaphore");
            }catch(Exception e)
            {
                MessageBox.Show($"Ошибка создания семафора. Сообщение - {e.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            /*Освобождаем семафор после создания*/
            semaphore.Release(1);
            /*Биндим сокет первого подпроцесса на локалхост и порт 20000*/
            subSocket1.Bind(new IPEndPoint(new IPAddress(new byte[4] { 127, 0, 0, 1 }), 20000));
            /*Создаём и конфигурируем первый подпроцесс*/
            subProcess1 = new Process();
            subProcess1.StartInfo.FileName = @"..\..\..\SubProcess1\bin\Debug\net6.0\SubProcess1.exe";
            if (!showConsole)
                subProcess1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            /*Запускаем первый подпроцесс*/
            subProcess1.Start();
            /*Слушаем первый сокет*/
            subSocket1.Listen(1);
            /*Принимаем подключение первого подпроцесса к сокету*/
            connectionSocket1 = subSocket1.Accept();
            /*Создаём левое изображение и графику*/
            leftImage = new Bitmap(leftPictureBox.Width, leftPictureBox.Height);
            leftPictureBox.Image = leftImage;
            leftGraphics = Graphics.FromImage(leftImage);
            /*передаём первому процессу параметры изображения через сокет*/
            connectionSocket1.Send(BitConverter.GetBytes(leftImage.Width));
            connectionSocket1.Send(BitConverter.GetBytes(leftImage.Height));
            /*Биндим сокет второго подпроцесса на локалхост и порт 20001*/
            subSocket2.Bind(new IPEndPoint(new IPAddress(new byte[4] { 127, 0, 0, 1 }), 20001));
            /*Создаём и настраиваем процесс аналогично первому*/
            subProcess2 = new Process();
            subProcess2.StartInfo.FileName = @"..\..\..\SubProcess2\bin\Debug\net6.0\SubProcess2.exe";
            if(!showConsole)
                subProcess2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            /*Запускаем подпроцесс*/
            subProcess2.Start();
            /*Слушаем второй сокет*/
            subSocket2.Listen(1);
            /*Принимаем подключение второго подпроцесса*/
            connectionSocket2 = subSocket2.Accept();
            /*Создаём правое изображение и графику*/
            rightImage = new Bitmap(rightPictureBox.Width, rightPictureBox.Height);
            rightPictureBox.Image = rightImage;
            rightGraphics = Graphics.FromImage(rightImage);
            /*Передаем параметры второму подпроцессу через сокет*/
            connectionSocket2.Send(BitConverter.GetBytes(rightImage.Width));
            connectionSocket2.Send(BitConverter.GetBytes(rightImage.Height));

        }


        /*Обработчик закрытия формы*/
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            DialogResult d = MessageBox.Show("Подтвердите выход", "Подтверждение", MessageBoxButtons.YesNo);
            if (d == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                subProcess1.Kill();
                subProcess2.Kill();
            }
        }


        /*Метод тика вызываемым таймером при запуске*/
        private void tick(object sender, EventArgs e)
        {
            
            /*Инициируем запуск генерации значений в подпроцессах*/
            if (firstTick)
            {
                //отправляем сообщение инициации на первый подпроцесс
                connectionSocket1.Send(buffer);
                //Ждём ответ о успешном захвате семафора и начале запуска генерации
                connectionSocket1.Receive(buffer);
                //отправляем сообщение инициации на второй подпроцесс
                connectionSocket2.Send(buffer);
                firstTick = false;
            }
            /*Очищение перед отрисовкой*/
            leftGraphics.Clear(Color.White);
            rightGraphics.Clear(Color.White);
            leftPictureBox.Image = leftImage;
            rightPictureBox.Image = rightImage;
            /*Получаем и парсим параметры от первого процесса - координаты x и y*/
            connectionSocket1.Receive(buffer);
            int left_x = BitConverter.ToInt32(buffer, 0);
            connectionSocket1.Receive(buffer);
            int left_y = BitConverter.ToInt32(buffer, 0);
            /*применяем параметры*/
            leftGraphics.DrawRectangle(new Pen(Color.Red, 2), left_x, left_y, leftImage.Width / 4, leftImage.Height / 4);
            leftPictureBox.Image = leftImage;
            /*отправляем сообщение о завершении операции, чтобы процесс освободил семафор*/
            connectionSocket1.Send(buffer);
            /*Получаем и парсим параметры от второго процесса - две пары координат x и y*/
            connectionSocket2.Receive(buffer);
            int right_x_1 = BitConverter.ToInt32(buffer, 0);
            connectionSocket2.Receive(buffer);
            int right_y_1 = BitConverter.ToInt32(buffer, 0);
            connectionSocket2.Receive(buffer);
            int right_x_2 = BitConverter.ToInt32(buffer, 0);
            connectionSocket2.Receive(buffer);
            int right_y_2 = BitConverter.ToInt32(buffer, 0);
            /*применяем параметры*/
            rightGraphics.DrawLine(new Pen(Color.Green, 2), right_x_1, right_y_1, right_x_2, right_y_2);
            rightPictureBox.Image = rightImage;
            /*отправляем сообщение о завершении операции, чтобы процесс освободил семафор*/
            connectionSocket2.Send(buffer);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }


    }
}
