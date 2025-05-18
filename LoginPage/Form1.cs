using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics.Eventing.Reader;
using MySql.Data.MySqlClient;


namespace LoginPage
{



    public partial class Form1 : Form
    {


        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;
        private bool resizing = false;
        private int borderSize = 6; // Grosor del borde para redimensionar
        private ResizeDirection resizeDirection;

        private void AplicarEsquinasRedondeadas(Panel panel, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            panel.Region = new Region(path);
        }

        private void CrearCamposFormulario()
        {
            int textBoxWidth = 200;
            int textBoxHeight = 25;
            int labelHeight = 20;
            int spacing = 10; // Espaciado entre controles
            int totalHeight = (textBoxHeight + labelHeight) * 3 + spacing * 2;
            int startY = (panel2.Height - totalHeight) / 2; // Posición vertical centrada
            int startX = (panel2.Width - textBoxWidth) / 2; // Posición horizontal centrada

            string[] etiquetas = { "Usename", "Email", "Password" };
            for (int i = 0; i < 3; i++)
            {
                Label label = new Label();
                label.Text = etiquetas[i];
                label.Size = new Size(label2.Width, label2.Height);
                label.Font = label2.Font;
                label.ForeColor = label2.ForeColor;
                label.BackColor = label2.BackColor;
                label.Location = new Point(startX, startY + (i * (textBoxHeight + labelHeight + spacing)));
                label.TextAlign = label2.TextAlign; // Usar la misma alineación

                TextBox textBox = new TextBox();
                textBox.Size = new Size(textBoxWidth, textBoxHeight);
                textBox.Location = new Point(startX, startY + labelHeight + (i * (textBoxHeight + labelHeight + spacing)));

                panel2.Controls.Add(label);
                panel2.Controls.Add(textBox);
            }
        }

        private void CrearBotonVolver()
        {
            Button volverBtn = new Button();
            volverBtn.Text = "Back";
            volverBtn.Size = register.Size; // Igual al botón register
            volverBtn.Font = register.Font;
            volverBtn.ForeColor = register.ForeColor;
            volverBtn.BackColor = Color.White; // Fondo blanco
            volverBtn.FlatStyle = register.FlatStyle;
            volverBtn.Location = new Point(register.Location.X, panel2.Height - 50); // Misma posición X

            volverBtn.Click += VolverAlPanel1;

            panel2.Controls.Add(volverBtn);
        }

        private void VolverAlPanel1(object sender, EventArgs e)
        {
            panel2.Visible = false;
            panel1.Visible = true;
            panel1.BringToFront();
        }


        private void ConectarBaseDeDatos()
        {
            string connectionString = "server=localhost;user=root;password=tu_contraseña;database=tu_base_de_datos";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Conexión exitosa.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        public Form1()
        {
            InitializeComponent();


            panel2 = new Panel
            {
                Size = panel1.Size,
                Location = panel1.Location,
                BackColor = panel1.BackColor,
                BorderStyle = BorderStyle.FixedSingle, // Aplicar borde negro
                ForeColor = panel1.ForeColor,
                BackgroundImage = panel1.BackgroundImage,
                BackgroundImageLayout = panel1.BackgroundImageLayout,
                Region = panel1.Region, // Copia las esquinas redondeadas
                Visible = false // Inicialmente oculto
            };

            panel2.Paint += Panel2_Paint; // Manejar el evento de dibujo
            this.Controls.Add(panel2);



            // Aplicar esquinas redondeadas a panel2
            AplicarEsquinasRedondeadas(panel2, 30);

            this.Controls.Add(panel2);

            // Copiar los controles de panel1 a panel2
            foreach (Control control in panel1.Controls)
            {
                Control newControl = (Control)Activator.CreateInstance(control.GetType());
                newControl.Size = control.Size;
                newControl.Location = control.Location;
                newControl.Text = control.Text;
                newControl.Font = control.Font;
                newControl.ForeColor = control.ForeColor;
                panel2.Controls.Add(newControl);
            }
            this.Controls.Add(panel2);

            // Configurar el campo de contraseña
            txtpass.UseSystemPasswordChar = true;


            this.FormBorderStyle = FormBorderStyle.None; // Sin bordes
            this.MinimumSize = new Size(300, 200); // Tamaño mínimo de la ventana

            // Manejo de eventos de arrastre
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;

            // Manejo de eventos en todos los controles
            foreach (Control control in this.Controls)
            {
                control.MouseDown += OnMouseDown;
                control.MouseMove += OnMouseMove;
                control.MouseUp += OnMouseUp;
            }

            // Manejo del redimensionamiento
            this.MouseDown += Resize_MouseDown;
            this.MouseMove += Resize_MouseMove;
            this.MouseUp += Resize_MouseUp;
        }

        // Método para dibujar un borde negro en panel2
        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel2.Width - 1, panel2.Height - 1);
            }
        }

        // Método para añadir el título "Register Panel"
        private void AgregarTituloPanel2()
        {
            Label titulo = new Label
            {
                Text = "Register Panel",
                Font = label1.Font,
                ForeColor = label1.ForeColor,
                Size = label1.Size,
                TextAlign = label1.TextAlign,
                Location = new Point(129, 35)
            };

            panel2.Controls.Add(titulo);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        // Mover la ventana
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !resizing)
            {
                isDragging = true;
                lastCursor = Cursor.Position;
                lastForm = Location;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int xDiff = Cursor.Position.X - lastCursor.X;
                int yDiff = Cursor.Position.Y - lastCursor.Y;
                Location = new Point(lastForm.X + xDiff, lastForm.Y + yDiff);
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // Redimensionar la ventana
        private void Resize_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && resizeDirection != ResizeDirection.None)
                resizing = true;
        }

        private void Resize_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizing)
            {
                int newWidth = Width;
                int newHeight = Height;

                if (resizeDirection.HasFlag(ResizeDirection.Right))
                    newWidth = Cursor.Position.X - Location.X;
                if (resizeDirection.HasFlag(ResizeDirection.Bottom))
                    newHeight = Cursor.Position.Y - Location.Y;
                if (resizeDirection.HasFlag(ResizeDirection.Left))
                {
                    int diff = Cursor.Position.X - Location.X;
                    newWidth -= diff;
                    Location = new Point(Location.X + diff, Location.Y);
                }
                if (resizeDirection.HasFlag(ResizeDirection.Top))
                {
                    int diff = Cursor.Position.Y - Location.Y;
                    newHeight -= diff;
                    Location = new Point(Location.X, Location.Y + diff);
                }

                this.Size = new Size(Math.Max(this.MinimumSize.Width, newWidth),
                                     Math.Max(this.MinimumSize.Height, newHeight));
            }
            else
            {
                resizeDirection = ResizeDirection.None;

                if (e.Location.X <= borderSize && e.Location.Y <= borderSize)
                {
                    Cursor = Cursors.SizeNWSE;
                    resizeDirection = ResizeDirection.Top | ResizeDirection.Left;
                }
                else if (e.Location.X >= Width - borderSize && e.Location.Y >= Height - borderSize)
                {
                    Cursor = Cursors.SizeNWSE;
                    resizeDirection = ResizeDirection.Bottom | ResizeDirection.Right;
                }
                else if (e.Location.X <= borderSize && e.Location.Y >= Height - borderSize)
                {
                    Cursor = Cursors.SizeNESW;
                    resizeDirection = ResizeDirection.Bottom | ResizeDirection.Left;
                }
                else if (e.Location.X >= Width - borderSize && e.Location.Y <= borderSize)
                {
                    Cursor = Cursors.SizeNESW;
                    resizeDirection = ResizeDirection.Top | ResizeDirection.Right;
                }
                else if (e.Location.X <= borderSize)
                {
                    Cursor = Cursors.SizeWE;
                    resizeDirection = ResizeDirection.Left;
                }
                else if (e.Location.X >= Width - borderSize)
                {
                    Cursor = Cursors.SizeWE;
                    resizeDirection = ResizeDirection.Right;
                }
                else if (e.Location.Y <= borderSize)
                {
                    Cursor = Cursors.SizeNS;
                    resizeDirection = ResizeDirection.Top;
                }
                else if (e.Location.Y >= Height - borderSize)
                {
                    Cursor = Cursors.SizeNS;
                    resizeDirection = ResizeDirection.Bottom;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void Resize_MouseUp(object sender, MouseEventArgs e)
        {
            resizing = false;
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {
            int radius = 30; // Radio de las esquinas
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(panel1.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(panel1.Width - radius, panel1.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, panel1.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                panel1.Region = new Region(path);

                using (Pen pen = new Pen(Color.Black, 2)) // Color y grosor del borde
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtuser.Text != "Username" && !string.IsNullOrWhiteSpace(txtuser.Text))
            {
                if (txtpass.Text != "Password" && !string.IsNullOrWhiteSpace(txtpass.Text))
                {
                    // Proceed with login logic (authentication, etc.)
                }
                else
                {
                    msgError("Please enter a valid password");
                }
            }
            else
            {
                msgError("Please enter a valid username");
            }

        }
        private void msgError(string msg)
        {
            lblErrorMessage.Text = msg;
            lblErrorMessage.Visible = false;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Ocultar Panel1 y mostrar Panel2 al frente
            panel1.Visible = false;
            panel2.Visible = true;
            panel2.BringToFront();

            //panel1.Controls.Clear();

            Panel registerPanel = new Panel();
            registerPanel.Location = panel1.Location;
            registerPanel.Size = panel1.Size;
            registerPanel.BackColor = Color.White;
            registerPanel.BorderStyle = BorderStyle.FixedSingle;

            int textBoxWidth = 200;
            int textBoxHeight = 20;
            int spacing = 10; // Espaciado entre los textBox
            int totalHeight = (textBoxHeight * 3) + (spacing * 2);
            int startY = (registerPanel.Height - totalHeight) / 2; // Posición vertical centrada
            int startX = (registerPanel.Width - textBoxWidth) / 2; // Posición horizontal centrada

            for (int i = 0; i < 3; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Size = new Size(textBoxWidth, textBoxHeight);
                textBox.Location = new Point(startX, startY + (i * (textBoxHeight + spacing)));
                registerPanel.Controls.Add(textBox);
            }

            panel1.Parent.Controls.Add(registerPanel);
            panel1.BringToFront();

            panel2.Controls.Clear();

            // Generar los TextBox y Labels centrados
            CrearCamposFormulario();

            // Crear el botón de "Volver"
            CrearBotonVolver();

            // Agregar el título
            AgregarTituloPanel2();

        }
    }

    [Flags]
    public enum ResizeDirection
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }
}