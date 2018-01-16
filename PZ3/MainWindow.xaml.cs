using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<CvorMape> sviCvorovi;
        private static List<List<Koordinata>> kooridinateLines;
        private static int brojacLinija;
        private GeometryModel3D hitgeo;
        private DiffuseMaterial blue = new DiffuseMaterial();

        private Point start = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 20;
        private int zoomCurent = 1;

        private static double ugaoPoX;
        private static double ugaoPoZ;
        public MainWindow()
        {
            sviCvorovi = new List<CvorMape>();
            kooridinateLines = new List<List<Koordinata>>();
            brojacLinija = 0;
            InitializeComponent();

            crtajCvorove();
        }

        private void crtajCvorove()
        {
            double xMin = 45.2325;  //19.72727  19.793909
            double xMax = 45.277031; //19.95094  19.894459
            double yMin = 19.793909; //45.18972  45.2325
            double yMax = 19.894459; //45.32873  45.277031
            double stopaX = 200/ (xMax - xMin);
            double stopaY =  200/ (yMax - yMin);


            Console.WriteLine("POCEO PARSIRANJE XML-A");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList substations = xmlDoc.GetElementsByTagName("SubstationEntity");
            XmlNodeList nodes = xmlDoc.GetElementsByTagName("NodeEntity");
            XmlNodeList switches = xmlDoc.GetElementsByTagName("SwitchEntity");
            XmlNodeList lines = xmlDoc.GetElementsByTagName("LineEntity");

            double latitude;
            double longitude;

            //za substations
            for (int i = 0; i < substations.Count; i++)
            {
                CvorMape tempSub = new CvorMape();
                XmlNodeList subChildNodes = substations[i].ChildNodes;


                ToLatLon(double.Parse(subChildNodes.Item(2).InnerText),
                            double.Parse(subChildNodes.Item(3).InnerText),
                            34,
                            out latitude,
                            out longitude);
                tempSub.X = latitude;
                tempSub.Y = longitude;
                tempSub.Id = subChildNodes.Item(0).InnerText.ToString();
                tempSub.Name = subChildNodes.Item(1).InnerText.ToString();
                tempSub.Type = "Substation Entity";
                sviCvorovi.Add(tempSub);
                //Console.WriteLine("x: " + subChildNodes.Item(2).InnerText + " y:" + subChildNodes.Item(3).InnerText + "id: " + subChildNodes.Item(0).InnerText);
                //Console.WriteLine("koord x: " + tempSub.X + " y:" + tempSub.Y + "id: " + tempSub.Id);
            }
            Console.WriteLine("ZAVRSIO PARSIRANJE SUBSTATIONS");

            //za nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                CvorMape tempNode = new CvorMape();
                XmlNodeList subChildNodes = nodes[i].ChildNodes;


                ToLatLon(double.Parse(subChildNodes.Item(2).InnerText.ToString()),
                            double.Parse(subChildNodes.Item(3).InnerText.ToString()),
                            34,
                            out latitude,
                            out longitude);
                tempNode.X = latitude;
                tempNode.Y = longitude;
                tempNode.Id = subChildNodes.Item(0).InnerText.ToString();
                tempNode.Name = subChildNodes.Item(1).InnerText.ToString();
                tempNode.Type = "Node Entity";
                sviCvorovi.Add(tempNode);
                //Console.WriteLine("x: " + subChildNodes.Item(2).InnerText + " y:" + subChildNodes.Item(3).InnerText + "id: " + subChildNodes.Item(0).InnerText);
                //Console.WriteLine("koord x: " + tempKoor.X + " y:" + tempKoor.Y);
            }
            Console.WriteLine("ZAVRSIO PARSIRANJE NODES");

            //za switches
            for (int i = 0; i < switches.Count; i++)
            {
                CvorMape tempSwitch = new CvorMape();
                XmlNodeList subChildNodes = switches[i].ChildNodes;


                ToLatLon(double.Parse(subChildNodes.Item(3).InnerText.ToString()),
                            double.Parse(subChildNodes.Item(4).InnerText.ToString()),
                            34,
                            out latitude,
                            out longitude);
                tempSwitch.X = latitude;
                tempSwitch.Y = longitude;
                tempSwitch.Id = subChildNodes.Item(0).InnerText.ToString();
                tempSwitch.Name = subChildNodes.Item(1).InnerText.ToString();
                tempSwitch.Type = "Switch Entity";
                sviCvorovi.Add(tempSwitch);
                //Console.WriteLine("x: " + subChildNodes.Item(3).InnerText + " y:" + subChildNodes.Item(4).InnerText + "id: " + subChildNodes.Item(0).InnerText);
                //Console.WriteLine("koord x: " + tempKoor.X + " y:" + tempKoor.Y);
            }
            Console.WriteLine("ZAVRSIO PARSIRANJE SWITCHES");

            //za lines
            List<Koordinata> lKord;
            for (int i = 0; i < lines.Count; i++)
            {
                lKord = new List<Koordinata>();

                XmlNodeList subChildNodes = lines[i].ChildNodes;

                XmlNode xnode = subChildNodes.Item(9);

                foreach (XmlNode xn in xnode.ChildNodes)
                {
                    Koordinata tempKoor = new Koordinata();
                    ToLatLon(double.Parse(xn.ChildNodes.Item(0).InnerText.ToString()),
                            double.Parse(xn.ChildNodes.Item(1).InnerText.ToString()),
                            34,
                            out latitude,
                            out longitude);
                    tempKoor.X = latitude;
                    tempKoor.Y = longitude;
                    //Console.WriteLine("koord x: " + tempKoor.X + " y:" + tempKoor.Y);
                    tempKoor.R = double.Parse(subChildNodes.Item(3).InnerText.ToString());
                    lKord.Add(tempKoor);
                }

                kooridinateLines.Add(lKord);
                //Console.WriteLine("x: " + subChildNodes.Item(3).InnerText + " y:" + subChildNodes.Item(4).InnerText);
                //Console.WriteLine("koord x: " + tempKoor.X + " y:" + tempKoor.Y);
            }
            Console.WriteLine("ZAVRSIO PARSIRANJE LINES");
            Console.WriteLine("ZAVRSIO PARSIRANJE XML-A");

            //iscrtavanje cvorova na mapu
            foreach (CvorMape cm in sviCvorovi)
            {
                GeometryModel3D cvorModel = new GeometryModel3D();
                MeshGeometry3D meshCvora = new MeshGeometry3D();

                if (cm.X > xMin && cm.X < xMax && cm.Y > yMin && cm.Y < yMax)
                {
                    // 0 0 0  2 0 0   2 0 2  0 0 2  1 2 1
                    double x = stopaX * (cm.X - xMin);
                    double y = stopaY * (cm.Y - yMin);
                    meshCvora.Positions.Add(new Point3D(x + 0, 0, y + 0)); //0
                    meshCvora.Positions.Add(new Point3D(x + 2, 0, y + 0)); //1
                    meshCvora.Positions.Add(new Point3D(x + 2, 0, y + 2)); //2
                    meshCvora.Positions.Add(new Point3D(x + 0, 0, y + 2)); //3
                    meshCvora.Positions.Add(new Point3D(x + 1, 2, y + 1)); //4

                    //0 1 2  0 2 3  2 1 4  2 4 3  0 4 1  3 4 0
                    meshCvora.TriangleIndices.Add(0);
                    meshCvora.TriangleIndices.Add(1);
                    meshCvora.TriangleIndices.Add(2);

                    meshCvora.TriangleIndices.Add(0);
                    meshCvora.TriangleIndices.Add(2);
                    meshCvora.TriangleIndices.Add(3);

                    meshCvora.TriangleIndices.Add(2);
                    meshCvora.TriangleIndices.Add(1);
                    meshCvora.TriangleIndices.Add(4);

                    meshCvora.TriangleIndices.Add(2);
                    meshCvora.TriangleIndices.Add(4);
                    meshCvora.TriangleIndices.Add(3);

                    meshCvora.TriangleIndices.Add(0);
                    meshCvora.TriangleIndices.Add(4);
                    meshCvora.TriangleIndices.Add(1);

                    meshCvora.TriangleIndices.Add(3);
                    meshCvora.TriangleIndices.Add(4);
                    meshCvora.TriangleIndices.Add(0);

                    cvorModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.HotPink));

                    cvorModel.Geometry = meshCvora;
                    grupica.Children.Add(cvorModel);
                    //brojac++;
                    // Console.WriteLine(brojac);
                }
            }

            //rotiranje
            //RotateTransform3D rot = new RotateTransform3D();
            //AxisAngleRotation3D rotacija = new AxisAngleRotation3D();
            //rotacija.Angle = 180;
            //rotacija.Axis = new Vector3D(0, 0, 1);
            //rot.Rotation = rotacija;
            //grupica.Transform = rot;
        }

        private void zoomMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(this);
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                kamera.Position = new Point3D(kamera.Position.X - 1, kamera.Position.Y - 1, kamera.Position.Z - 1);
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                kamera.Position = new Point3D(kamera.Position.X + 1, kamera.Position.Y + 1, kamera.Position.Z + 1);
            }
            //Point p = e.MouseDevice.GetPosition(this);
            //double scaleX = 1;
            //double scaleY = 1;
            //if (e.Delta > 0 && zoomCurent < zoomMax)
            //{
            //    scaleX = skaliranje.ScaleX + 0.1;
            //    scaleY = skaliranje.ScaleY + 0.1;
            //    zoomCurent++;
            //    skaliranje.ScaleX = scaleX;
            //    skaliranje.ScaleY = scaleY;
            //}
            //else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            //{
            //    scaleX = skaliranje.ScaleX - 0.1;
            //    scaleY = skaliranje.ScaleY - 0.1;
            //    zoomCurent--;
            //    skaliranje.ScaleX = scaleX;
            //    skaliranje.ScaleY = scaleY;
            //}

        }

        private void panMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vjuport.CaptureMouse();
            start = e.GetPosition(this);

            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;

            rotacijaWheel(vjuport, e);
        }

        private void panMouseMove(object sender, MouseEventArgs e)
        {
            if (vjuport.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                //if (offsetX < 0 && offsetY < 0)
                //{
                    kamera.Position = new Point3D(kamera.Position.X - offsetX / 3000, kamera.Position.Y, kamera.Position.Z - offsetY / 3000);
                //}
                //else if (offsetX < 0 && offsetY > 0)
                //{
                //    kamera.Position = new Point3D(kamera.Position.X - offsetX / 50000, kamera.Position.Y, kamera.Position.Z + offsetY / 50000);
                //}
                //else if (offsetX > 0 && offsetY < 0)
                //{
                //    kamera.Position = new Point3D(kamera.Position.X + offsetX / 50000, kamera.Position.Y, kamera.Position.Z - offsetY / 50000);
                //}
                //else if (offsetX > 0 && offsetY > 0)
                //{
                //    kamera.Position = new Point3D(kamera.Position.X + offsetX / 50000, kamera.Position.Y, kamera.Position.Z + offsetY / 50000);
                //}
            }
            else if(vjuport.IsMouseCaptured && e.MiddleButton == MouseButtonState.Pressed)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                //kamera.Position = new Point3D(kamera.Position.X + offsetX / 300, kamera.Position.Y +  offsetY / 300, kamera.Position.Z + offsetY / 300);
                //kamera.LookDirection = new Vector3D(kamera.LookDirection.X - offsetX / 300, kamera.LookDirection.Y - offsetY / 300, kamera.LookDirection.Z - offsetY / 300);
                mapaPoX.Angle = ugaoPoX - offsetX*100 / w;
                mapaPoZ.Angle = ugaoPoZ - offsetY*100 / h;

                elementiPoX.Angle = mapaPoX.Angle;
                elementiPoZ.Angle = mapaPoZ.Angle;

                prveLinijePoX.Angle = mapaPoX.Angle;
                prveLinijePoZ.Angle = mapaPoZ.Angle;

                drugeLinijePoX.Angle = mapaPoX.Angle;
                drugeLinijePoZ.Angle = mapaPoZ.Angle;

                treceLinijePoX.Angle = mapaPoX.Angle;
                treceLinijePoZ.Angle = mapaPoZ.Angle;
            }

            //**************************************************************
            //if (vjuport.IsMouseCaptured)
            //{
            //    Point end = e.GetPosition(this);
            //    double offsetX = end.X - start.X;
            //    double offsetY = end.Y - start.Y;
            //    double w = this.Width;
            //    double h = this.Height;
            //    double translateX = (offsetX * 100) / w;
            //    double translateY = -(offsetY * 100) / h;

            //    translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
            //    translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));

            //}

            //if (vjuport.IsMouseCaptured)
            //{
            //    Point end = e.GetPosition(this);
            //    double offsetX = end.X - start.X;
            //    double offsetY = end.Y - start.Y;
            //    double w = this.Width;
            //    double h = this.Height;
            //    double translateX = (offsetX * 10) / w*10;
            //    double translateY = -(offsetY * 10) / h*10;
            //    kamera.Position = new Point3D(kamera.Position.X + translateX , kamera.Position.Y, kamera.Position.Z + translateY );

            //}
        }

        private void panMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vjuport.ReleaseMouseCapture();
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void prviChecked(object sender, RoutedEventArgs e)
        {
            double xMin = 45.2325;  //19.72727  19.793909
            double xMax = 45.277031; //19.95094  19.894459
            double yMin = 19.793909; //45.18972  45.2325
            double yMax = 19.894459; //45.32873  45.277031
            double stopaX = 200 / (xMax - xMin);
            double stopaY = 200 / (yMax - yMin);
            foreach (List<Koordinata> lk in kooridinateLines)
            {


                for (int i = 0; i < (lk.Count - 1); i++) //
                {
                    GeometryModel3D cvorModel = new GeometryModel3D();
                    MeshGeometry3D meshCvora = new MeshGeometry3D();
                    if (lk[i].X > xMin && lk[i].X < xMax && lk[i].Y > yMin && lk[i].Y < yMax && lk[i + 1].X > xMin && lk[i + 1].X < xMax && lk[i + 1].Y > yMin && lk[i + 1].Y < yMax && lk[i].R < 1)
                    {

                        double x1 = stopaX * (lk[i].X - xMin);
                        double y1 = stopaY * (lk[i].Y - yMin);
                        double x2 = stopaX * (lk[i + 1].X - xMin);
                        double y2 = stopaY * (lk[i + 1].Y - yMin);

                        Vector3D v = new Vector3D(x2 - x1, 0, y2 - y1);
                        Vector3D up = new Vector3D(0, 1, 0);
                        Vector3D normala = Vector3D.CrossProduct(v, up);


                        double intenzitet = Math.Sqrt(normala.X * normala.X + normala.Y * normala.Y + normala.Z * normala.Z);

                        normala = normala / intenzitet;

                        Point3D a = new Point3D(x1, 0, y1);
                        Point3D p1 = new Point3D(normala.X + x1, 0, normala.Z + y1);
                        Point3D b = new Point3D(x2, 0, y2);
                        Point3D p2 = new Point3D(normala.X + x2, 0, normala.Z + y2);

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3


                        meshCvora.TriangleIndices.Add(0);
                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(3);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(4);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(5);

                        meshCvora.TriangleIndices.Add(5);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(7);
                        //*****************************************************
                        //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 0)); //0
                        //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 0)); //1
                        //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 1.3)); //2
                        //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 1.3)); //3

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(0);

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(2);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(3);

                        //****************************************************



                        //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 + 0.3)); //1 0
                        //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 + 0.3)); //0 1
                        //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 - 0.3)); //3 2
                        //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 - 0.3)); //2 3

                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 + 0.3)); //1 4
                        //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 + 0.3)); //0 5
                        //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 - 0.3)); //3 6
                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 - 0.3)); //2 7

                        //meshCvora.TriangleIndices.Add(6);
                        //meshCvora.TriangleIndices.Add(5);
                        //meshCvora.TriangleIndices.Add(1);

                        //meshCvora.TriangleIndices.Add(6);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(2);

                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(5);
                        //meshCvora.TriangleIndices.Add(6);

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(6);

                        //meshCvora.TriangleIndices.Add(4);
                        //meshCvora.TriangleIndices.Add(7);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.TriangleIndices.Add(4);
                        //meshCvora.TriangleIndices.Add(3);
                        //meshCvora.TriangleIndices.Add(0);

                        //meshCvora.TriangleIndices.Add(3);
                        //meshCvora.TriangleIndices.Add(7);
                        //meshCvora.TriangleIndices.Add(4);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(3);
                        //meshCvora.TriangleIndices.Add(4);

                        cvorModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkGreen));

                        cvorModel.Geometry = meshCvora;
                        prveLinije.Children.Add(cvorModel);
                        brojacLinija++;
                    }


                }

            }
            //iscrtajPrve(kooridinateLines);
            //Thread.Sleep(500);
            //Console.WriteLine("ukupno: " + brojacLinija);
        }

        //private void iscrtajPrve(List<List<Koordinata>> kordLines)
        //{
        //    double xMin = 45.2325;  //19.72727  19.793909
        //    double xMax = 45.277031; //19.95094  19.894459
        //    double yMin = 19.793909; //45.18972  45.2325
        //    double yMax = 19.894459; //45.32873  45.277031
        //    double stopaX = 200 / (xMax - xMin);
        //    double stopaY = 200 / (yMax - yMin);
        //    foreach (List<Koordinata> lk in kordLines)
        //    {
                

        //        for (int i = 0; i < (lk.Count-1); i++) //
        //        {
        //            GeometryModel3D cvorModel = new GeometryModel3D();
        //        MeshGeometry3D meshCvora = new MeshGeometry3D();
        //            if (lk[i].X > xMin && lk[i].X < xMax && lk[i].Y > yMin && lk[i].Y < yMax && lk[i + 1].X > xMin && lk[i + 1].X < xMax && lk[i + 1].Y > yMin && lk[i + 1].Y < yMax && lk[i].R < 1)
        //            {

        //                crtanje(cvorModel, meshCvora, lk[i].X, lk[i].Y, lk[i+1].X, lk[i+1].Y, xMin, yMin, stopaX, stopaY);
        //            }


        //        }

        //    }
        //}

        //private void crtanje(GeometryModel3D cvorModel, MeshGeometry3D meshCvora,double lkx1, double lky1, double lkx2, double lky2, double xMin, double yMin, double stopaX, double stopaY )
        //{
        //    double x1 = stopaX * (lkx1 - xMin);
        //    double y1 = stopaY * (lky1 - yMin);
        //    double x2 = stopaX * (lkx2 - xMin);
        //    double y2 = stopaY * (lky2 - yMin);

        //    Vector3D v = new Vector3D(x2 - x1, 0, y2 - y1);
        //    Vector3D up = new Vector3D(0, 1, 0);
        //    Vector3D normala = Vector3D.CrossProduct(v, up);


        //    double intenzitet = Math.Sqrt(normala.X * normala.X + normala.Y * normala.Y + normala.Z * normala.Z);

        //    normala = normala / intenzitet;

        //    Point3D a = new Point3D(x1, 0, y1);
        //    Point3D p1 = new Point3D(normala.X + x1, 0, normala.Z + y1);
        //    Point3D b = new Point3D(x2, 0, y2);
        //    Point3D p2 = new Point3D(normala.X + x2, 0, normala.Z + y2);

        //    meshCvora.Positions.Add(a); //0
        //    meshCvora.Positions.Add(p1); // 1
        //    meshCvora.Positions.Add(b); //2
        //    meshCvora.Positions.Add(p2); //3

        //    meshCvora.Positions.Add(a); //0
        //    meshCvora.Positions.Add(p1); // 1
        //    meshCvora.Positions.Add(b); //2
        //    meshCvora.Positions.Add(p2); //3


        //    meshCvora.TriangleIndices.Add(0);
        //    meshCvora.TriangleIndices.Add(1);
        //    meshCvora.TriangleIndices.Add(2);

        //    meshCvora.TriangleIndices.Add(1);
        //    meshCvora.TriangleIndices.Add(3);
        //    meshCvora.TriangleIndices.Add(2);

        //    meshCvora.TriangleIndices.Add(4);
        //    meshCvora.TriangleIndices.Add(6);
        //    meshCvora.TriangleIndices.Add(5);

        //    meshCvora.TriangleIndices.Add(5);
        //    meshCvora.TriangleIndices.Add(6);
        //    meshCvora.TriangleIndices.Add(7);
        //    //*****************************************************
        //    //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 0)); //0
        //    //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 0)); //1
        //    //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 1.3)); //2
        //    //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 1.3)); //3

        //    //meshCvora.TriangleIndices.Add(2);
        //    //meshCvora.TriangleIndices.Add(1);
        //    //meshCvora.TriangleIndices.Add(0);

        //    //meshCvora.TriangleIndices.Add(2);
        //    //meshCvora.TriangleIndices.Add(0);
        //    //meshCvora.TriangleIndices.Add(3);

        //    //meshCvora.TriangleIndices.Add(0);
        //    //meshCvora.TriangleIndices.Add(1);
        //    //meshCvora.TriangleIndices.Add(2);

        //    //meshCvora.TriangleIndices.Add(0);
        //    //meshCvora.TriangleIndices.Add(2);
        //    //meshCvora.TriangleIndices.Add(3);

        //    //****************************************************



        //    //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 + 0.3)); //1 0
        //    //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 + 0.3)); //0 1
        //    //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 - 0.3)); //3 2
        //    //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 - 0.3)); //2 3

        //    //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 + 0.3)); //1 4
        //    //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 + 0.3)); //0 5
        //    //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 - 0.3)); //3 6
        //    //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 - 0.3)); //2 7

        //    //meshCvora.TriangleIndices.Add(6);
        //    //meshCvora.TriangleIndices.Add(5);
        //    //meshCvora.TriangleIndices.Add(1);

        //    //meshCvora.TriangleIndices.Add(6);
        //    //meshCvora.TriangleIndices.Add(1);
        //    //meshCvora.TriangleIndices.Add(2);

        //    //meshCvora.TriangleIndices.Add(1);
        //    //meshCvora.TriangleIndices.Add(5);
        //    //meshCvora.TriangleIndices.Add(6);

        //    //meshCvora.TriangleIndices.Add(2);
        //    //meshCvora.TriangleIndices.Add(1);
        //    //meshCvora.TriangleIndices.Add(6);

        //    //meshCvora.TriangleIndices.Add(4);
        //    //meshCvora.TriangleIndices.Add(7);
        //    //meshCvora.TriangleIndices.Add(3);

        //    //meshCvora.TriangleIndices.Add(4);
        //    //meshCvora.TriangleIndices.Add(3);
        //    //meshCvora.TriangleIndices.Add(0);

        //    //meshCvora.TriangleIndices.Add(3);
        //    //meshCvora.TriangleIndices.Add(7);
        //    //meshCvora.TriangleIndices.Add(4);

        //    //meshCvora.TriangleIndices.Add(0);
        //    //meshCvora.TriangleIndices.Add(3);
        //    //meshCvora.TriangleIndices.Add(4);

        //    cvorModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkGreen));

        //    cvorModel.Geometry = meshCvora;
        //    prveLinije.Children.Add(cvorModel);
        //    brojacLinija++;
        //}

        private void drugiChecked(object sender, RoutedEventArgs e)
        {
            double xMin = 45.2325;  //19.72727  19.793909
            double xMax = 45.277031; //19.95094  19.894459
            double yMin = 19.793909; //45.18972  45.2325
            double yMax = 19.894459; //45.32873  45.277031
            double stopaX = 200 / (xMax - xMin);
            double stopaY =200 / (yMax - yMin);
            foreach (List<Koordinata> lk in kooridinateLines)
            {
               

                for (int i = 0; i < (lk.Count - 1); i++)
                {
                    GeometryModel3D cvorModel = new GeometryModel3D();
                    MeshGeometry3D meshCvora = new MeshGeometry3D();
                    if (lk[i].X > xMin && lk[i].X < xMax && lk[i].Y > yMin && lk[i].Y < yMax && lk[i + 1].X > xMin && lk[i + 1].X < xMax && lk[i + 1].Y > yMin && lk[i + 1].Y < yMax && lk[i].R > 1 &&  lk[i].R < 2)
                    {
                        // 0 0 0  2 0 0   2 0 2  0 0 2  1 2 1
                        double x1 = stopaX * (lk[i].X - xMin);
                        double y1 = stopaY * (lk[i].Y - yMin);
                        double x2 = stopaX * (lk[i + 1].X - xMin);
                        double y2 = stopaY * (lk[i + 1].Y - yMin);

                        Vector3D v = new Vector3D(x2 - x1, 0, y2 - y1);
                        Vector3D up = new Vector3D(0, 1, 0);
                        Vector3D normala = Vector3D.CrossProduct(v, up);


                        double intenzitet = Math.Sqrt(normala.X * normala.X + normala.Y * normala.Y + normala.Z * normala.Z);

                        normala = normala / intenzitet;

                        Point3D a = new Point3D(x1, 0, y1);
                        Point3D p1 = new Point3D(normala.X + x1, 0, normala.Z + y1);
                        Point3D b = new Point3D(x2, 0, y2);
                        Point3D p2 = new Point3D(normala.X + x2, 0, normala.Z + y2);

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3


                        meshCvora.TriangleIndices.Add(0);
                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(3);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(4);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(5);

                        meshCvora.TriangleIndices.Add(5);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(7);
                        //double x1 = stopaX * (lk[i].X - xMin);
                        //double y1 = stopaY * (lk[i].Y - yMin);
                        //double x2 = stopaX * (lk[i + 1].X - xMin);
                        //double y2 = stopaY * (lk[i + 1].Y - yMin);

                        //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 0)); //0
                        //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 0)); //1
                        //meshCvora.Positions.Add(new Point3D(y2 + 1.3, -1.1, x2 + 1.3)); //2
                        //meshCvora.Positions.Add(new Point3D(y1 + 0, -1.1, x1 + 1.3)); //3

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(0);

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(2);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(3);



                        cvorModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Tomato));

                        cvorModel.Geometry = meshCvora;
                        drugeLinije.Children.Add(cvorModel);
                        brojacLinija++;
                    }
                }
            }
            //Console.WriteLine("ukupno: " + brojacLinija);
        }

        private void treciChecked(object sender, RoutedEventArgs e)
        {
            double xMin = 45.2325;  //19.72727  19.793909
            double xMax = 45.277031; //19.95094  19.894459
            double yMin = 19.793909; //45.18972  45.2325
            double yMax = 19.894459; //45.32873  45.277031
            double stopaX = 200 / (xMax - xMin);
            double stopaY = 200 / (yMax - yMin);
            foreach (List<Koordinata> lk in kooridinateLines)
            {
                

                for (int i = 0; i < (lk.Count - 1); i++)
                {
                    GeometryModel3D cvorModel = new GeometryModel3D();
                    MeshGeometry3D meshCvora = new MeshGeometry3D();
                    if (lk[i].X > xMin && lk[i].X < xMax && lk[i].Y > yMin && lk[i].Y < yMax && lk[i + 1].X > xMin && lk[i + 1].X < xMax && lk[i + 1].Y > yMin && lk[i + 1].Y < yMax && lk[i].R > 2)
                    {
                        // 0 0 0  2 0 0   2 0 2  0 0 2  1 2 1

                        double x1 = stopaX * (lk[i].X - xMin);
                        double y1 = stopaY * (lk[i].Y - yMin);
                        double x2 = stopaX * (lk[i + 1].X - xMin);
                        double y2 = stopaY * (lk[i + 1].Y - yMin);

                        Vector3D v = new Vector3D(x2 - x1, 0, y2 - y1);
                        Vector3D up = new Vector3D(0, 1, 0);
                        Vector3D normala = Vector3D.CrossProduct(v, up);


                        double intenzitet = Math.Sqrt(normala.X * normala.X + normala.Y * normala.Y + normala.Z * normala.Z);

                        normala = normala / intenzitet;

                        Point3D a = new Point3D(x1, 0, y1);
                        Point3D p1 = new Point3D(normala.X + x1, 0, normala.Z + y1);
                        Point3D b = new Point3D(x2, 0, y2);
                        Point3D p2 = new Point3D(normala.X + x2, 0, normala.Z + y2);

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3

                        meshCvora.Positions.Add(a); //0
                        meshCvora.Positions.Add(p1); // 1
                        meshCvora.Positions.Add(b); //2
                        meshCvora.Positions.Add(p2); //3


                        meshCvora.TriangleIndices.Add(0);
                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(1);
                        meshCvora.TriangleIndices.Add(3);
                        meshCvora.TriangleIndices.Add(2);

                        meshCvora.TriangleIndices.Add(4);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(5);

                        meshCvora.TriangleIndices.Add(5);
                        meshCvora.TriangleIndices.Add(6);
                        meshCvora.TriangleIndices.Add(7);
                        //double x1 = stopaX * (lk[i].X - xMin);
                        //double y1 = stopaY * (lk[i].Y - yMin);
                        //double x2 = stopaX * (lk[i + 1].X - xMin);
                        //double y2 = stopaY * (lk[i + 1].Y - yMin);
                        //meshCvora.Positions.Add(new Point3D(x1 + 0, 0.1, y1 + 0)); //0
                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 + 0)); //1
                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 + 0.3)); //2
                        //meshCvora.Positions.Add(new Point3D(x1 + 0, 0.1, y1 + 0.3)); //3

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(0);

                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(2);

                        //meshCvora.TriangleIndices.Add(0);
                        //meshCvora.TriangleIndices.Add(2);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 + 0.3)); //1 0
                        //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 + 0.3)); //0 1
                        //meshCvora.Positions.Add(new Point3D(x1 - 0.3, 0.1, y1 - 0.3)); //3 2
                        //meshCvora.Positions.Add(new Point3D(x1 + 0.3, 0.1, y1 - 0.3)); //2 3

                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 + 0.3)); //1 4
                        //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 + 0.3)); //0 5
                        //meshCvora.Positions.Add(new Point3D(x2 - 0.3, 0.1, y2 - 0.3)); //3 6
                        //meshCvora.Positions.Add(new Point3D(x2 + 0.3, 0.1, y2 - 0.3)); //2 7

                        //meshCvora.TriangleIndices.Add(6);
                        //meshCvora.TriangleIndices.Add(5);
                        //meshCvora.TriangleIndices.Add(1);

                        //meshCvora.TriangleIndices.Add(6);
                        //meshCvora.TriangleIndices.Add(1);
                        //meshCvora.TriangleIndices.Add(2);

                        //meshCvora.TriangleIndices.Add(4);
                        //meshCvora.TriangleIndices.Add(7);
                        //meshCvora.TriangleIndices.Add(3);

                        //meshCvora.TriangleIndices.Add(4);
                        //meshCvora.TriangleIndices.Add(3);
                        //meshCvora.TriangleIndices.Add(0);

                        cvorModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.MidnightBlue));

                        cvorModel.Geometry = meshCvora;
                        treceLinije.Children.Add(cvorModel);
                        brojacLinija++;
                    }
                }
            }
            //Console.WriteLine("ukupno: " + brojacLinija);
        }

        private void prviUnchecked(object sender, RoutedEventArgs e)
        {
            prveLinije.Children.Clear();
        }

        private void drugiUnchecked(object sender, RoutedEventArgs e)
        {
            drugeLinije.Children.Clear();
        }

        private void treciUnchecked(object sender, RoutedEventArgs e)
        {
            treceLinije.Children.Clear();
        }

        private void rotacijaWheel(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                vjuport.CaptureMouse();
                start = e.GetPosition(this);

                diffOffset.X = translacija.OffsetX;
                diffOffset.Y = translacija.OffsetY;

                ugaoPoX = mapaPoX.Angle;
                ugaoPoZ = mapaPoZ.Angle;

            }
            else
            {
                System.Windows.Point mouseposition = e.GetPosition(vjuport);
                Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0); //dodali z kao trecu koordinatu da bi bilo 3d 
                Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

                PointHitTestParameters pointparams =
                         new PointHitTestParameters(mouseposition);
                RayHitTestParameters rayparams =
                         new RayHitTestParameters(testpoint3D, testdirection);

                //test for a result in the Viewport3D     
                hitgeo = null; //tu se smesta kvadar koji som pogodili
                VisualTreeHelper.HitTest(vjuport, null, HTResult, pointparams);
            }
        }

        private HitTestResultBehavior
                HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {

                DiffuseMaterial darkSide =
                     new DiffuseMaterial(new SolidColorBrush(
                     System.Windows.Media.Colors.Red));
                bool gasit = false;
                for (int i = 1; i < grupica.Children.Count; i++)
                {
                    if ((GeometryModel3D)grupica.Children[i] == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit; //tu se nalazi kvadar koji smo pogopdili
                        gasit = true;

                        MeshGeometry3D hitMesh = (MeshGeometry3D)hitgeo.Geometry;
                        double xMin = 45.2325;  //19.72727  19.793909
                        double xMax = 45.277031; //19.95094  19.894459
                        double yMin = 19.793909; //45.18972  45.2325
                        double yMax = 19.894459; //45.32873  45.277031
                        double stopaX = 200 / (xMax - xMin);
                        double stopaY = 200 / (yMax - yMin);

                        Point3D p = hitMesh.Positions[0];
                        double x = p.X;
                        double y = p.Z;

                        foreach (CvorMape cm in sviCvorovi)
                        {
                            if(x == (stopaX * (cm.X - xMin)) && y == (stopaY * (cm.Y - yMin)))
                            {
                                MessageBox.Show("name: " + cm.Name + " id: " + cm.Id + " type: " + cm.Type);
                            }
                        }
                    }
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop; //enumeracija, to znaci da hocemo da zaustavimo zrak, a ne da se nastavlja
        }

        private void rotacijaWheelUp(object sender, MouseButtonEventArgs e)
        {
            vjuport.ReleaseMouseCapture();
        }
    }
}
