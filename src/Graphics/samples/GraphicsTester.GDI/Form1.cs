using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.GDI;
using System.Windows.Forms;
using GraphicsTester.Scenarios;

namespace GraphicsTester.GDI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            gdiGraphicsView1.Renderer = new GDIDirectGraphicsRenderer() {BackgroundColor = Colors.White};
            foreach (var scenario in ScenarioList.Scenarios)
            {
                listBox1.Items.Add(scenario);
            }

            listBox1.SelectedIndexChanged += delegate(object sender, EventArgs args)
            {
                var item = ScenarioList.Scenarios[listBox1.SelectedIndex];
                gdiGraphicsView1.Drawable = item;
            };
        }
    }
}
