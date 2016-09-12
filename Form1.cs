using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShapesGraphics
{
    [Flags]
    public enum ShapeType { LINE      = 1 << 0,
                            CIRCLE    = 1 << 1,
                            RECTANGLE = 1 << 2, 
                            FREE      = 1 << 3  };  
          
    public partial class Form1 : Form
    {
        const string APP_NAME         = "Shapes Graphics ";
        const string ALL_SHAPES       = "All shapes";
        const string LINES_ONLY       = "Lines only";
        const string CIRCLES_ONLY     = "Circles only";
        const string RECTANGLES_ONLY  = "Rectangles only";
        const string FREESHAPE_ONLY   = "Free draw only";
        const int    FREESHAPE_BYPASS = 99999;
        // The bypass value is used when the mouse-up event of the free-drawing is OUTSIDE of the panel 
        // But we still want to add the shape in the mouse click event....

        Pen             pen;
        Brush           brush;
        Image           image;
        Color           currentColor;
        HashSet<Button> shapesButtonsSet;

        ShapeContainer  sc;              // the shapes container
        ShapeType       chosenShape;     // the current chosen shape to draw

        int             clickEvent;      // to know if first click or second click
        Point           startPoint;      // x,y of first click
        Point           endPoint;        // x,y of second click                
        
        Shape           shapeClicked;    // the shape under the right click
        Shape           clonedShape;     // for copy/paste !
        Point           rightClickPoint; // right click location

        bool            dirty;           // for file save...
        string          saveFileName;    // the chosen file name to save to
        string          imageFileName;   // for filling shape with image 

        bool            freeDraw;        // for mouse free drawing
        int             prevX, prevY;    // used for free drawing
        List<Point>     freeDrawShape;   // to store all the x y from free drawing before adding to container
        
        public Form1()
        {
            // 1. Init Form
            InitializeComponent();

            // 2. Init Form Data
            sc = new ShapeContainer(50);

            chosenShape = ShapeType.LINE;  
            pictureBoxCurrentColor.BackColor = currentColor = Color.DarkCyan;                                                       
            pen   = new Pen(currentColor,int.Parse(comboBoxPenWidth.Text));
            brush = new SolidBrush(currentColor);
            image = null;
            clickEvent = 0;
            dirty      = false;

            // 3. Init Tool Bars
            this.statusBarToolStripMenuItem.Image = ShapesGraphics.Properties.Resources.GreenCheck;
            this.pasteToolStripMenuItem.Enabled = false;
            
            // 4. Init Shapes Buttons
            shapesButtonsSet = new HashSet<Button>();
            shapesButtonsSet.Add(buttonLine);
            shapesButtonsSet.Add(buttonCircle);
            shapesButtonsSet.Add(buttonRectangle);
            shapesButtonsSet.Add(buttonFree);                       

            // 5. Create bitmaps for Buttons pictures...           
            Bitmap bmpLine = new Bitmap(50, 50);
            Graphics gg = Graphics.FromImage(bmpLine);
            gg.Clear(Color.Transparent);
            gg.DrawLine(new Pen(Color.DarkBlue, 4), new System.Drawing.Point(14, 36), new System.Drawing.Point(36, 14));            
            buttonLine.Image = bmpLine;

            Bitmap bmpCircle = new Bitmap(50, 50);
            gg = Graphics.FromImage(bmpCircle);
            gg.Clear(Color.Transparent);
            gg.DrawEllipse(new Pen(Color.DarkBlue, 4), 12, 12, 26, 26);
            buttonCircle.Image = bmpCircle;

            Bitmap bmpRectangle = new Bitmap(50, 50);
            gg = Graphics.FromImage(bmpRectangle);
            gg.Clear(Color.Transparent);
            gg.DrawRectangle(new Pen(Color.DarkBlue, 4), 12, 13, 25, 25);
            buttonRectangle.Image = bmpRectangle;

            // 6. Subscribe to the shapes container area events !
            SubscribeToAreaEvents();
        }

        private void SubscribeToAreaEvents()
        {
            // 6.1 Original way
            //sc.tooSmallAreaEvent += new Action<bool>(areaTooSmallHandler);
            //sc.tooLargeAreaEvent += new Action<bool>(areaTooLargeHandler);

            // 6.2 Using anonymous method
            sc.tooSmallAreaEvent += delegate(bool value) { textBoxDecrease.Enabled = !value; };

            // 6.3 Using Lanbda
            sc.tooLargeAreaEvent += (value) => { textBoxIncrease.Enabled = !value; };
        }
        //private void areaTooSmallHandler(bool value)
        //{
        //    // if (value == true) it means that the area of the smalles shape is less than the minimum             
        //    textBoxDecrease.Enabled = ! value; // 
        //}
        //private void areaTooLargeHandler(bool value)
        //{
        //    // if (value == true) it means that the area of the largest shape is greater than the maximum     
        //    textBoxIncrease.Enabled = ! value;
        //}

        private void buttonChooseColor_Click(object sender, EventArgs e)
        {
            // Open color dialog
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (colorDialog1.Color == Color.White)
                {
                    MessageBox.Show("White color will not be seen, choose another color...", "Note");
                    colorDialog1.Color = currentColor;
                }
                else
                {
                    pictureBoxCurrentColor.BackColor = currentColor = colorDialog1.Color;
                    pictureBoxColor1.BackColor = currentColor;
                }
            }
            clickEvent = 0;
        }
        private void pictureBoxCurrentColor_Click(object sender, EventArgs e)
        {
            buttonChooseColor_Click(sender, e);
        }

        private void comboBoxViewAll_SelectedValueChanged(object sender, EventArgs e)
        {
            // Go over List and set show/hide param             
            ShapeType type = ShapeType.LINE | ShapeType.CIRCLE | ShapeType.RECTANGLE | ShapeType.FREE; // The 'All' option                      
            switch (comboBoxViewAll.Text)
            {
                case LINES_ONLY:
                    {                        
                        type = ShapeType.LINE;
                        if (chosenShape != ShapeType.LINE) buttonLine_Click(sender, e);
                        break;
                    }
                case CIRCLES_ONLY:
                    {
                        type = ShapeType.CIRCLE;
                        if (chosenShape != ShapeType.CIRCLE) buttonCircle_Click(sender, e);
                        break;
                    }
                case RECTANGLES_ONLY:
                    {
                        type = ShapeType.RECTANGLE;
                        if (chosenShape != ShapeType.RECTANGLE) buttonRectangle_Click(sender, e);
                        break;
                    }
                case FREESHAPE_ONLY:
                    {
                        type = ShapeType.FREE;
                        if (chosenShape != ShapeType.FREE) buttonFree_Click(sender, e);
                        break;
                    }
            }

            sc.SetShowType(type);
            labelViewStyle.Text = (comboBoxViewAll.Text.Equals(ALL_SHAPES)) ? "" : comboBoxViewAll.Text + " view";
            panel1.Refresh();
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
             // Clear all 
             sc.RemoveAll();
             // Init static input order count
             Shape.InputOrder = 0;
             PostChangeActions();
             clickEvent = 0;                 
        }
        private void buttonRemoveLast_Click(object sender, EventArgs e)
        {
            // Remove last only if in 'all shapes view'
            if (comboBoxViewAll.Text == ALL_SHAPES)
            {
                // Clear only last shape entered ( the one with the highest input order ...)            
                sc.RemoveLastInput();
                PostChangeActions();
                clickEvent = 0;            
            }
            else            
                MessageBox.Show("'All Shapes View' must be enabled before removing last shape !","Note:");            
        }

        private void buttonLine_Click(object sender, EventArgs e)
        {
            chosenShape = ShapeType.LINE;
            SetShapesButtonsColor(buttonLine);
            checkBoxFillShape.Checked = false;
            checkBoxFillShape.Enabled = false;
            SetLineStyleButtons(true);
            // set default order viewing options before continue drawing..
            SetDevaultOrderView();
            string viewType = comboBoxViewAll.Text;
            if (viewType.Equals(CIRCLES_ONLY) || viewType.Equals(RECTANGLES_ONLY) || viewType.Equals(FREESHAPE_ONLY))
                comboBoxViewAll.Text = ALL_SHAPES;
            clickEvent = 0;
        }
        private void buttonCircle_Click(object sender, EventArgs e)
        {
            chosenShape = ShapeType.CIRCLE;
            SetShapesButtonsColor(buttonCircle);
            SetLineStyleButtons(true);

            checkBoxFillShape.Enabled = true;
            radioButtonImage.Enabled = false;
            labelRed.Visible = false;
            if (radioButtonImage.Checked) radioButtonSolidFill.Checked = true;

            // set default order viewing options before continue drawing..
            SetDevaultOrderView();
            string viewType = comboBoxViewAll.Text;
            if (viewType.Equals(LINES_ONLY) || viewType.Equals(RECTANGLES_ONLY) || viewType.Equals(FREESHAPE_ONLY))
                comboBoxViewAll.Text = ALL_SHAPES;
            clickEvent = 0;
        }
        private void buttonRectangle_Click(object sender, EventArgs e)
        {
            chosenShape = ShapeType.RECTANGLE;
            SetShapesButtonsColor(buttonRectangle);
            SetLineStyleButtons(true);
            checkBoxFillShape.Enabled = true;
            radioButtonImage.Enabled = true;
            // set default order viewing options before continue drawing..
            SetDevaultOrderView();
            string viewType = comboBoxViewAll.Text;
            if (viewType.Equals(LINES_ONLY) || viewType.Equals(CIRCLES_ONLY) || viewType.Equals(FREESHAPE_ONLY))
                comboBoxViewAll.Text = ALL_SHAPES;
            clickEvent = 0;
        }
        private void buttonFree_Click(object sender, EventArgs e)
        {
            chosenShape = ShapeType.FREE;
            SetShapesButtonsColor(buttonFree);
            checkBoxFillShape.Checked = false;
            checkBoxFillShape.Enabled = false;
            SetLineStyleButtons(false);
            // set default order viewing options before continue drawing..
            SetDevaultOrderView();
            string viewType = comboBoxViewAll.Text;
            if (viewType.Equals(LINES_ONLY) || viewType.Equals(CIRCLES_ONLY) || viewType.Equals(RECTANGLES_ONLY))
                comboBoxViewAll.Text = ALL_SHAPES;
            clickEvent = 0;
        }

        private void SetDevaultOrderView()
        {
            if (!radioButtonViewByOrder.Checked)
                radioButtonViewByOrder.Checked = true;            
        }
        private void SetLineStyleButtons(bool value)
        {
            radioButtonDashDot.Enabled = value;
            radioButtonDashDotDot.Enabled = value;
            radioButtonDottedLine.Enabled = value;
        }

        private void SetShapesButtonsColor(Button btn)
        {
            foreach (Button b in shapesButtonsSet)
                if (b.Equals(btn))
                    b.BackColor = Color.SandyBrown;
                else
                    b.BackColor = SystemColors.Control;                
        }
       
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            bool shapeWasAdded = false;

            // Convert screen points to control points
            System.Drawing.Point p1 = panel1.PointToClient(Control.MousePosition);

            // *** Right Click ***
            if (e.Button == MouseButtons.Right)
            {
                // If Shape detected under -OR- there is a cloned shape for 'Paste' action, then open context menu
                rightClickPoint = new Point(p1.X, p1.Y);
                shapeClicked = sc.GetShapeAtPosition(rightClickPoint);
                if ((shapeClicked != null) || (clonedShape != null))                  
                {  
                    // 1. Set views 
                    if (shapeClicked != null)
                    {
                        // 1.1 Show details on status bar   
                        statusStrip1.Items[3].Text = shapeClicked.ToString();
                        statusStrip1.Items[3].ForeColor = Color.Firebrick;
                        SetStatusBarView(false, true);

                        // 1.2 Set view options in context menu
                        copyToolStripMenuItem.Enabled = cutToolStripMenuItem.Enabled = removeToolStripMenuItem.Enabled = true;

                        if (sc.Size() > 1)
                            sendToBackToolStripMenuItem.Enabled = bringToFrontToolStripMenuItem.Enabled = true;
                        else
                            sendToBackToolStripMenuItem.Enabled = bringToFrontToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        copyToolStripMenuItem.Enabled = cutToolStripMenuItem.Enabled = removeToolStripMenuItem.Enabled =
                        sendToBackToolStripMenuItem.Enabled = bringToFrontToolStripMenuItem.Enabled = false;
                    }    

                    // 2. Open context menu 
                    contextMenuStrip1.Show(panel1, p1);
                    clickEvent = 0;
                }                
            }
            // *** Left Click ***
            else
            {
                // Find the control where the mouse click occured                
                // var hwnd = WindowFromPoint(Control.MousePosition);
                var hwnd = WindowFromPoint(Cursor.Position);
                var c = Control.FromHandle(hwnd);
                if ((c == panel1) || (e.Clicks == FREESHAPE_BYPASS))
                {
                    if (clickEvent == 0)
                    {
                        // 1. This is the first click
                        clickEvent = 1;
                        // 2. Only record the x,y position of mouse
                        startPoint = new Point(p1.X, p1.Y);
                    }
                    else if (clickEvent == 1)
                    {
                        // 1. This is the second click
                        clickEvent = 0;

                        // Go back to this viewing option before continue drawing..
                        if (!radioButtonViewByOrder.Checked)
                            radioButtonViewByOrder.Checked = true;

                        // 2. Now record the x,y position of mouse
                        endPoint = new Point(p1.X, p1.Y);

                        string showType = comboBoxViewAll.Text;                       

                        // 3. Now add the chosen shape to shape container
                        switch (chosenShape)
                        {
                            case ShapeType.LINE:
                                {
                                    SetPenBrushImage(1, 1);
                                    Line line = new Line(startPoint, currentColor, endPoint, pen, brush);
                                    line.Show = (showType == ALL_SHAPES) || (showType == LINES_ONLY) ? true : false;
                                    sc.Add(line);
                                    shapeWasAdded = true;
                                    break;
                                }
                            case ShapeType.CIRCLE:
                                {
                                    // Swap points if drawing from right to left..
                                    Point tempE = endPoint;
                                    Point tempS = startPoint;

                                    if ((endPoint.X < startPoint.X) && (endPoint.Y >= startPoint.Y))
                                    {
                                        startPoint.X = tempE.X;
                                        endPoint.X = tempS.X;
                                    }
                                    if ((endPoint.X < startPoint.X) && (endPoint.Y < startPoint.Y))
                                    {
                                        startPoint = tempE;
                                        endPoint = tempS;
                                    }
                                    if ((endPoint.X > startPoint.X) && (endPoint.Y < startPoint.Y))
                                    {
                                        startPoint.Y = tempE.Y;
                                        endPoint.Y = tempS.Y;
                                    }

                                    float radius = endPoint.Distance(startPoint);
                                    if (radius != 0) // if radius is != 0
                                    {
                                        SetPenBrushImage((int)radius, (int)radius);
                                        Circle circle = new Circle(startPoint, currentColor, checkBoxFillShape.Checked, pen, brush, radius, image);
                                        circle.Show = (showType == ALL_SHAPES) || (showType == CIRCLES_ONLY) ? true : false;
                                        sc.Add(circle);
                                        shapeWasAdded = true;                                       
                                    }
                                    else
                                        // Consider click as not performed...since circle with 0 radius is NOT added.
                                        clickEvent = 1;

                                    break;
                                }
                            case ShapeType.RECTANGLE:
                                {
                                    // Swap points if drawing from right to left..
                                    if (endPoint.X < startPoint.X)
                                    {
                                        int temp = endPoint.X;
                                        endPoint.X = startPoint.X;
                                        startPoint.X = temp;
                                    }
                                    if (endPoint.Y < startPoint.Y)
                                    {
                                        int temp = endPoint.Y;
                                        endPoint.Y = startPoint.Y;
                                        startPoint.Y = temp;
                                    }

                                    int width = endPoint.X - startPoint.X;
                                    int height = endPoint.Y - startPoint.Y;

                                    if ((width != 0) && (height != 0))
                                    {
                                        try
                                        {
                                            // Exception will be thrown if image file was not selected for image fill type
                                            SetPenBrushImage(width, height);
                                            Rectangle rect = new Rectangle(startPoint, currentColor, checkBoxFillShape.Checked, pen, brush,
                                                                           width, height, image);
                                            rect.Show = (showType == ALL_SHAPES) || (showType == RECTANGLES_ONLY) ? true : false;
                                            sc.Add(rect);
                                            shapeWasAdded = true;
                                        }
                                        catch
                                        {
                                            // Nothing to do, shape will just not be added, user will just need to select an image file
                                        }
                                    }
                                    else
                                        // Consider click as not performed...since recangle with 0 width/height is NOT added.
                                        clickEvent = 1;

                                    break;
                                }
                            case ShapeType.FREE:
                                {
                                    if (freeDrawShape.Count > 1)
                                    {
                                        SetPenBrushImage(1, 1);
                                        FreeDraw freeShape = new FreeDraw(new Point(freeDrawShape[0].X,freeDrawShape[0].Y), 
                                                                          currentColor, freeDrawShape, pen, brush);
                                        freeShape.Show = (showType == ALL_SHAPES) || (showType == FREESHAPE_ONLY) ? true : false;
                                        sc.Add(freeShape);
                                        shapeWasAdded = true;
                                    }
                                    else
                                        // Consider click as not performed...since free shape with 1 cell length is NOT added.
                                        clickEvent = 1;
                                    break;
                                }
                            default:
                                {
                                    MessageBox.Show("Invalid Shape Passed");
                                    break; 
                                }
                        }

                        // 4. Post addition actions
                        if (shapeWasAdded)
                        {
                            PostChangeActions();
                        }
                    }
                }                
            }
        }

        private void PostChangeActions()
        {
            // 5. Sort shapes according to the view option chosen
            if (radioButtonViewSmallestOnTop.Checked)
                sc.Sort(new SortLargeToSmall()); // smallest on top
            else if (radioButtonViewLargestOnTop.Checked)
                sc.Sort();                       // largest on top

            // 6. Refresh panel to redraw shapes
            panel1.Refresh();
            dirty = true;
            if (this.Text[this.Text.Length - 1] != '*')
                this.Text += '*';
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {       
            // Draw Grid
            if (this.gridLinesToolStripMenuItem.Image != null)
                DrawGrid(Color.LightGray);
            else
                DrawGrid(panel1.BackColor);   

            // Draw Shapes
            Graphics g = panel1.CreateGraphics();                                      
            sc.DrawAll(g);

            // Events are used instead of the following 2 lines...
            //textBoxIncrease.Enabled = (sc.LargestArea > ShapeContainer.MAX_AREA) ? false : true;
            //textBoxDecrease.Enabled = (sc.SmallestArea < ShapeContainer.MIN_AREA) ? false : true;

            groupBoxViewOptions.Focus();
        }
 
        private void textBoxIncrease_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 1. Prevent the 'Ding' sound
                e.Handled = true;
                e.SuppressKeyPress = true;

                // 2. Call Resize                        
                PercentChange(textBoxIncrease);
            }
            clickEvent = 0;
        }
        private void textBoxDecrease_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 1. Prevent the 'Ding' sound
                e.Handled = true;
                e.SuppressKeyPress = true;

                // 2. Call Resize                
                PercentChange(textBoxDecrease);
            }
            clickEvent = 0;
        }
        private void PercentChange(TextBox box)
        {
                // Go over List and resize all accordingly      
                int maxVal = box.Equals(textBoxDecrease) ? 99 : 500;
                int percent;

                if (int.TryParse(box.Text, out percent) && (percent >= 1) && (percent <= maxVal))
                {
                    labelInvalidPercent1.Visible = false;
                    labelInvalidPercent2.Visible = false;

                    percent = box.Equals(textBoxDecrease) ? (percent * -1) : percent;
                    sc.ResizeAll(percent);

                    panel1.Refresh();
                    dirty = true;
                }
                else
                {
                    if (box.Equals(textBoxDecrease))
                        labelInvalidPercent2.Visible = true;
                    else
                        labelInvalidPercent1.Visible = true;
                    SystemSounds.Beep.Play();
                    MessageBox.Show("Please enter a number between 1 - " + maxVal.ToString(), "Resize");
                }                        
        }
        
        private void radioButtonViewByOrder_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonViewByOrder.Checked == true)
            {
                // Sort Shapes Container (largest on bottom, smallest on top) and Redraw !
                sc.Sort(new SortInputOrder());
                panel1.Refresh();
            }
            clickEvent = 0;
        }
        private void radioButtonViewSmallestOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonViewSmallestOnTop.Checked == true)
            {
                // Sort Shapes Container (largest on bottom, smallest on top) and Redraw !
                sc.Sort(new SortLargeToSmall());
                panel1.Refresh();
            }
            clickEvent = 0;
        }
        private void radioButtonViewLargestOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonViewLargestOnTop.Checked == true)
            {
                // Sort Shapes Container (smallest on bottom, largest on top) and Redraw !
                sc.Sort();
                panel1.Refresh();
            }
            clickEvent = 0;
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toggle status bar on / off           
            statusStrip1.Visible = !statusStrip1.Visible;
            this.statusBarToolStripMenuItem.Image = statusStrip1.Visible ?
                           ShapesGraphics.Properties.Resources.GreenCheck : null;
            clickEvent = 0;
        }
        private void gridLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.gridLinesToolStripMenuItem.Image == null)
            {
                this.gridLinesToolStripMenuItem.Image = ShapesGraphics.Properties.Resources.GreenCheck;
                DrawGrid(Color.LightGray);               
            }
            else
            {
                this.gridLinesToolStripMenuItem.Image = null;
                DrawGrid(panel1.BackColor);                   
            }
            panel1.Refresh();
        }
        private void DrawGrid(Color color)
        {
            int thick = 1;          
            int i = 0;

            while (i < panel1.Size.Height)
            {
                // Draw horizontal lines
                DrawLine(color, thick, 1, i, panel1.Size.Width, i);
                i += 20;
            }

            i = 0;
            while (i < panel1.Size.Width)
            {
                // Draw vertical lines
                DrawLine(color, thick, i, 1, i, panel1.Size.Height);
                i += 20;
            }
        }
        private void DrawLine(Color color, int thick, int x1, int y1, int x2, int y2)
        {
            Graphics g;
            using (g = panel1.CreateGraphics())
            using (Pen pen = new Pen(color, thick))
            {
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }  
               
        private void SetPenBrushImage(int w, int h)
        {
            pen   = new Pen(currentColor, int.Parse(comboBoxPenWidth.Text));
            brush = new SolidBrush(currentColor);
            image = null;

            if (checkBoxFillShape.Checked)
            {
                // Brush type                
                if (radioButtonHatchFill.Checked)
                {   
                    brush = new HatchBrush(RandomEnumValue<HatchStyle>(), pictureBoxColor1.BackColor, pictureBoxColor2.BackColor);
                }
                if (radioButtonGradient.Checked)
                {
                    System.Drawing.RectangleF r = new System.Drawing.Rectangle(0, 0, new Random().Next(1, w+1), new Random().Next(1, h+1));
                    LinearGradientMode lgm = RandomEnumValue<LinearGradientMode>();
                    //LinearGradientMode lgm = LinearGradientMode.Vertical;
                    brush = new LinearGradientBrush(r, pictureBoxColor1.BackColor, pictureBoxColor2.BackColor, lgm);                   
                }
                if (radioButtonImage.Checked)
                {                    
                    try
                    {
                        if (string.IsNullOrEmpty(imageFileName))
                        {
                            labelRed.Visible = true;
                            throw new ShapesException("Image file not chosen, Please choose an image file");
                        }
                        else
                            image = new Bitmap(imageFileName);              
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message,"Error");
                        throw e;
                    }
              }
            }
            else
            {
                // Pen type               
                pen.DashStyle = DashStyle.Solid;
                if (radioButtonDottedLine.Checked) pen.DashStyle = DashStyle.Dot;
                if (radioButtonDashDot.Checked)    pen.DashStyle = DashStyle.DashDot;
                if (radioButtonDashDotDot.Checked) pen.DashStyle = DashStyle.DashDotDot;
            }
        }
        private T RandomEnumValue<T>()
        {
            Array arr = Enum.GetValues(typeof(T));
            return (T)arr.GetValue(new Random().Next(arr.Length));
        }
        
        private void comboBoxViewAll_KeyPress(object sender, KeyPressEventArgs e)
        {
            // to prevent user input
            e.Handled = true;
        }
        private void comboBoxPenWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            // to prevent user input
            e.Handled = true;
        }
        
        private void checkBoxFillShape_CheckStateChanged(object sender, EventArgs e)
        { 
            groupBoxChooseBrush.Visible = (checkBoxFillShape.Checked == true) ? true : false;
            groupBoxChoosePen.Visible   = (checkBoxFillShape.Checked == true) ? false : true;
            clickEvent = 0;   
        }

        private void radioButtonSolidFill_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxSet2Colors.Visible = false;
            buttonImageBrowse.Visible = false;
            labelRed.Visible = false;
            clickEvent = 0;
        }
        private void radioButtonHatchFill_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxSet2Colors.Visible = true;
            buttonImageBrowse.Visible = false;
            labelRed.Visible = false;
            clickEvent = 0;
        }
        private void radioButtonGradient_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxSet2Colors.Visible = true;
            buttonImageBrowse.Visible = false;
            labelRed.Visible = false;
            clickEvent = 0;
        }
        private void radioButtonImage_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxSet2Colors.Visible = false;
            buttonImageBrowse.Visible = true;
            buttonImageBrowse.Enabled = true;
            clickEvent = 0;
        }

        private void groupBoxSet2Colors_VisibleChanged(object sender, EventArgs e)
        {
            if (groupBoxSet2Colors.Visible)
                pictureBoxColor1.BackColor = currentColor;
        }
        private void pictureBoxColor1_Click(object sender, EventArgs e)
        {
            // Open color dialog
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBoxColor1.BackColor = pictureBoxCurrentColor.BackColor = currentColor = colorDialog1.Color;                
            }
            clickEvent = 0;
        }
        private void pictureBoxColor2_Click(object sender, EventArgs e)
        {
            // Open color dialog
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBoxColor2.BackColor = colorDialog1.Color;               
            }
            clickEvent = 0;
        }

        private void buttonImageBrowse_Click(object sender, EventArgs e)
        {
            // Open file dialog to choose image
            openFileDialog1.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                imageFileName = openFileDialog1.FileName;
                labelRed.Visible = false;
            }
        }

        private void SetStatusBarView(bool a, bool b)
        {
            statusStrip1.Items[0].Visible = a;
            statusStrip1.Items[1].Visible = a;
            statusStrip1.Items[2].Visible = b;
            statusStrip1.Items[3].Visible = b;     
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            if (dirty)
            {
                bool needToSave = true;

                if (string.IsNullOrEmpty(saveFileName))
                {                  
                    // Open file dialog  
                    saveFileDialog1.Title = "Save File: ";
                    saveFileDialog1.Filter = "dat files|*.dat";
                    saveFileDialog1.FileName = "";
                    needToSave = OpenTheSaveDialog();
                }

                if (needToSave) 
                {
                    try
                    {
                        SaveToFile();
                        dirty = false;
                    }
                    catch(IOException ex)
                    {
                        MessageBox.Show("Error Occured :: " + ex.Message, "Error");
                    }
                }            
            } 
        }
        private bool OpenTheSaveDialog()
        {
            bool needToSave = true;
          
            DialogResult result = saveFileDialog1.ShowDialog();                      

            if ((result == DialogResult.OK) && (saveFileDialog1.FileName != ""))               
            {
                if  (Path.GetExtension(saveFileDialog1.FileName).Equals(".dat"))
                {
                     saveFileName = saveFileDialog1.FileName;
                     this.Text = APP_NAME + saveFileName;
                }
                else
                {
                    // Trying to save with wrong file type
                    MessageBox.Show("File type must be  *.dat","Note");
                    needToSave = false;
                }               
            }
            else // If cancel for example was pressed...
                needToSave = false;

            return needToSave;
        }
        private void SaveToFile()
        {          
            try
            {
                // Create file, write (serialize) & close                    
                BinaryFormatter binFormatter = new BinaryFormatter();
                using (Stream fStream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormatter.Serialize(fStream, sc);
                }
                // Update form title
                this.Text = APP_NAME + saveFileName;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show("Serialization Error :: " + ex.Message, "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occured :: " + ex.Message, "Error");
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult askResult = DialogResult.No;

            // 1. If 'dirty' ....save first
            if (dirty)
            {
                askResult = AskIfToSave(sender, e);
            }
                        
            if (askResult == DialogResult.No)
            {
                 // 2. Open existing file
                 openFileDialog1.Filter = "dat files|*.dat";
                 openFileDialog1.FileName = "";
                 DialogResult result = openFileDialog1.ShowDialog();
                 if ((result == DialogResult.OK) && (openFileDialog1.FileName != ""))
                 {
                     bool success = false;

                     string fName = openFileDialog1.FileName;
                     this.Text = APP_NAME + saveFileName;
                     clickEvent = 0;
                                         
                     ShapeContainer tempContainer = sc;

                     try
                     {
                         // 3. Read data from file to container                         
                         BinaryFormatter binFormatter = new BinaryFormatter();
                         using (Stream fStream = new FileStream(fName, FileMode.Open, FileAccess.Read, FileShare.None))
                         {
                             tempContainer = (ShapeContainer)binFormatter.Deserialize(fStream);
                             success = true;
                         }
                     }
                     catch (SerializationException)
                     {
                         MessageBox.Show("De-Serializaion Error Occured :: Invalid File Format", "Error");
                     }
                     catch (Exception ex)
                     {
                         MessageBox.Show("Error Occured :: " + ex.Message, "Error");
                     }

                     if (success)
                     {
                         // 4. Now we have the container
                         //    Note: 
                         //    If error occured and file was not de-serialized than sc gets original data that was saved in the temp var
                         sc = tempContainer;

                         // 5. Resubscribe to class events !
                         SubscribeToAreaEvents();
                                               
                         // 6. Set view settings
                         this.Text = saveFileName = fName;
                         SetDevaultOrderView();
                         comboBoxViewAll.Text = ALL_SHAPES;
                         panel1.Refresh();
                         dirty = false;
                     }
                 }
            }            
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. If 'dirty' ....save first
            if (dirty)
                AskIfToSave(sender, e);
          
            // 2. Clear all data
            saveFileName = "";
            this.Text = APP_NAME;
            sc.RemoveAll();            
            clickEvent = 0;
            panel1.Refresh();
            dirty = false;
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Save File As: ";
            saveFileDialog1.Filter = "dat files|*.dat";
            saveFileDialog1.FileName = "";
            if (OpenTheSaveDialog())
            {
                try
                {
                    SaveToFile();
                    dirty = false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Error Occured :: " + ex.Message, "Error");
                }
            }
        }
        private DialogResult AskIfToSave(object sender, EventArgs e)
        {
             DialogResult result =  MessageBox.Show("There are unsaved shapes." + 
                                                       Environment.NewLine + "Do you want to save first ?", "Note", 
                                                       MessageBoxButtons.YesNo);
             if (result == DialogResult.Yes)
             {
                 saveToolStripMenuItem_Click(sender, e);
             }

             return result;
        }
        private void saveFileAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Capture panel1 area (before poping dialog which covers the screen...)
            System.Drawing.Rectangle rect;
            rect = new System.Drawing.Rectangle(panel1.PointToScreen(new System.Drawing.Point(0, 0)), panel1.Size);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            // Open Save File Dialog
            saveFileDialog1.Title = "Save Drawing As Image: ";
            saveFileDialog1.Filter = "JPG files|*.jpg";
            saveFileDialog1.FileName = "";
            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) && (saveFileDialog1.FileName != ""))               
            {
                if  (Path.GetExtension(saveFileDialog1.FileName).Equals(".jpg"))
                {
                    try
                    {
                        // Actual Save to jpg file                                    
                        bmp.Save(saveFileDialog1.FileName);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Error Occured :: " + ex.Message, "Error");
                    }
                }
                else
                {
                    MessageBox.Show("File type should be  *.jpg", "Note");
                }
            }           
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1_FormClosing(sender, new FormClosingEventArgs(CloseReason.UserClosing, false));
        }        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = DialogResult.Yes;

            // 1. Check dirty before exit....
            if (dirty)
                result = AskIfToSave(sender, e);

            // 2. Exit...            
            //Environment.Exit(0);
            Application.Exit();
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Items[1].Text = "";
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            // 1. Show mouse coordinates at bottom status bar
            if (!statusStrip1.Items[1].Visible)
                SetStatusBarView(true, false);

            statusStrip1.Items[1].Text = panel1.PointToClient(Control.MousePosition).ToString();

            // 2. Draw if 'free draw'  
            if ((chosenShape == ShapeType.FREE) && (freeDraw))
            {
                Graphics g;
                using (g = panel1.CreateGraphics())                
                {
                    //  2.A   Draw on screen for viewing
                    g.DrawLine(new Pen(currentColor, int.Parse(comboBoxPenWidth.Text)), prevX, prevY, e.X, e.Y);
                    prevX = e.X;
                    prevY = e.Y;
                    //  2.B   Add point to the the dynamically created free draw object
                    //        This object will be added to shape container upoin mouse up ....
                    //        when drawing is done
                    freeDrawShape.Add(new Point(prevX, prevY));
                }
            }
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            // The following is needed for free draw..
            if (e.Button == MouseButtons.Left)
                if (chosenShape == ShapeType.FREE)
                {
                    freeDraw = true;                
                    // The start point value is taken upon mouse down
                    prevX = e.X; 
                    prevY = e.Y;
                    // Init the object to hold the free shape
                    freeDrawShape = new List<Point>(1000);
                    // Add the first point to the object
                    freeDrawShape.Add(new Point(prevX, prevY));
                }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {   
            // Add free draw shape to shape container
            if (freeDraw)
            {
                // If mouse up was OUTSIDE of panel1 then:
                if (e.X < 0 || e.X > panel1.Width || e.Y < 0 || e.Y > panel1.Height)
                {
                    // Take out the last negative values in the free draw object
                    int i = freeDrawShape.Count-1;
                    while ((freeDrawShape[i].X < 0 || freeDrawShape[i].Y < 0) && (i >= 0))
                    {
                        i--;
                    }                  
                    freeDrawShape = freeDrawShape.GetRange(0, i - 1);
                }

                // Now do the object addition to the container, sending special number (FREESHAPE_BYPASS = 99999) in the e clicks param....
                clickEvent = 1;
                panel1_MouseClick(sender, new MouseEventArgs(MouseButtons.Left,FREESHAPE_BYPASS,
                                                             freeDrawShape[freeDrawShape.Count - 1].X,freeDrawShape[freeDrawShape.Count - 1].Y,1));
            }

            // Clean data
            freeDraw = false;
            freeDrawShape = null;
        }
        
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. shapeClicked is the shape under mouse position            
            // 2. Clone shape and hold it in memory (prev shape in memeory is replaced)
            clonedShape = (Shape)shapeClicked.Clone();           
            
            // 3. Enable 'paste' in menu
            pasteToolStripMenuItem.Enabled = true;
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. if there is a shape in memory then add it to container
            if (clonedShape != null)
            {              
                // 1.0 clone the clonedShape as it is ONLY the model, when there are multiple 'paste' actions !
                Shape shapeToAdd = (Shape)clonedShape.Clone();

                // 1.1 Modify x,y of shape according to paste mouse position !                              
                shapeToAdd.MoveLocation(rightClickPoint);
                
                // 1.2 Add
                shapeToAdd.shapeInputOrder = Shape.InputOrder++;
                sc.Add(shapeToAdd);

                // 1.3 Do post addition actions                
                PostChangeActions();
            }
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Remove the shape clicked 
            sc.Remove(shapeClicked);
            PostChangeActions();
        } 
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. Put clicked shape in memeory - copy action
            copyToolStripMenuItem_Click(sender, e);

            // 2. Remove the clicked shape
            removeToolStripMenuItem_Click(sender, e);
          
            // 3. Refresh is done by remove action..                   
        }
        
        private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This action will change the input order

            // 1. Set shape order to be the first one (0)
            sc.ChangeShapeInputOrder(shapeClicked, 0);

            // 2. Sort & view by input order
            radioButtonViewByOrder.Checked = true;                       
            sc.Sort(new SortInputOrder());
            
            PostChangeActions();

        }
        private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This action will change the input order

            // 1. Set shape order ot be the last (top)
            sc.ChangeShapeInputOrder(shapeClicked, 1);

            // 2. Sort & view by input order
            radioButtonViewByOrder.Checked = true;     
            sc.Sort(new SortInputOrder());
           
            PostChangeActions();
        }
        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

    }
}
