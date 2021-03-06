﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows.Forms;

namespace MapEditor
{
    public class Map
    {

       

        public void SetBackground(string filename)
        {
            Bitmap temp = new Bitmap(filename);
            backBit = temp;//Utils.InteligentResize(temp);
            if (backgraundOn)
            {
                specials[BACKGROUND].Remove();
                
            }
            Texture txt = new Texture(BACKGROUND, 0, filename, true);
            txt.Load();
            specials[BACKGROUND] = txt;
            backgraundOn = true;

        }
        public abstract class MapLoadingState
        {
            public abstract void Load(string[] line, Map map);


            protected void SetField(int i, int j, Field f, Map m)
            {
                m.SetField(i, j, f);
            }
            protected void ChangeState(Map m, MapLoadingState state)
            {
                m.state = state;
            }
            protected void ThrowLoadException()
            {
                throw new Exception();
            }
            protected bool Check(string[] line)
            {
                return line.Length <= 0;
            }
            protected void CreateField(int textid, int index , Map m)
            {
                Field f = new Field(m.textures[textid], false);
                int x = index%m.Width;
                int y = index/m.Width;
                m.SetField(x, y, f);
            }
            protected void SetSize(int width, int height, Map m)
            {
                m.Height = height;
                m.Width = width;
                m.fields = new Field[width*height];
            }
            protected void SetStartPoint(int x, int y, Map m)
            {
                m.StartPosition = new Point(x/Field.SIZE, y/Field.SIZE);
            }
        }

        public event EventHandler Loaded;
        private MapLoadingState state;
        private const int DEFAULT_WIDTH = 100;
        private const int DEFAULT_HEIGHT = 100;

        private static string texturesFile = "tiles.txt";
        private static string texturesPath = "tiles";
        private static string maps = "maps";
        private static string specialTextures = "textures";
        private static string startPositionTexture = "startPosition.png";
        private static string background = "background.png";
        private static string triggerImage = "trigger.png";
        private const int START = 0;
        private const int BACKGROUND = 1;
        private const int TRIGGER = 2;

        public bool ShowTriggers = false;
        public string mapfolder;
        public string basepath;

        private Bitmap backBit;

        private bool backgraundOn = false;
        private bool startPositionSeted;
        private Point startPosition;
        public Point StartPosition { get { return startPosition; } set { startPosition = value; startPositionSeted = true; } }

        Field[] fields;
        public Dictionary<int,Texture> textures;
        public Dictionary<int, Texture> specials;
      

        public int Width;
        public int Height;
        public double RealWidth { get { return Width * Field.SIZE; } }
        public double RealHeight { get { return Height * Field.SIZE; } }
        private string name;

        

        public Map(int width, int height, string name)
        {
            this.mapfolder = string.Empty;//Path.Combine(Application.StartupPath,maps,
            this.basepath = Application.StartupPath;
            textures = new Dictionary<int, Texture>();
            specials = new Dictionary<int, Texture>();
            fields = new Field[width * height];
            Width = width;
            Height = height;
            this.name = name;
            LoadTextures(false);
            CreateTiles();
            backgraundOn = false;
            EditorEngine.Instance.CreaturesManager.LoadPrototypes(String.Empty);
            

        }

        private void CreateTiles()
        {
           
            for (int i = 0; i < Height * Width; i++)
            {
                Field f = new Field(textures[0], false);
                fields[i] = f;
            }

        }

        public Map(string name)
        {
            this.mapfolder = Path.Combine(Application.StartupPath,maps,name);
            this.basepath = Application.StartupPath;
            textures = new Dictionary<int, Texture>();
            specials = new Dictionary<int, Texture>();
            this.name = Path.GetFileName(mapfolder);
            LoadTextures(true);
            LoadBackground();
            LoadTiles();
            EditorEngine.Instance.CreaturesManager.LoadPrototypes(mapfolder);
            EditorEngine.Instance.TriggetManager.LoadTriggers(mapfolder,this);
            if (Loaded != null) Loaded(this, EventArgs.Empty);


        }
        private void LoadTiles()
        {
            string mapfile = Path.Combine(mapfolder, "map");
            state = MapLoadingStateSize.Instance;
            try
            {
                using (StreamReader sr = new StreamReader(mapfile))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] line = sr.ReadLine().TrimEnd('\r', '\t', ' ', '\n').Split(' ');
                        state.Load(line, this);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new MapLoadException("Cannot find map file (" + mapfile + ")", ex);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new MapLoadException("Incorrect format", ex);
            }
            catch (Exception ex)
            {
                throw new MapLoadException("Exception during loading a map", ex);
            }
        }
        public Field GetField(int x, int y)
        {
            return fields[y * Width + x];
        }
        public void SetField(int x, int y, Field f)
        {
            fields[y * Width + x] = f;
        }
        public void LoadTextures(bool loadmap)
        {
            loadTextures(basepath,true);
            if(loadmap) loadTextures(mapfolder,false);
            foreach (KeyValuePair<int, Texture> pair in textures)
            {
                pair.Value.Load();
            }
            loadSpecials();

            
        }
        private void LoadBackground()
        {
            Texture background = new Texture(BACKGROUND, 0, Path.Combine(mapfolder, Map.background), true);
            background.Load();
            specials[BACKGROUND] = background;
            backgraundOn = true;
            backBit = background.Bmp;
        }
        private void loadSpecials()
        {
            Texture start = new Texture(START, 0, Path.Combine(basepath, specialTextures, startPositionTexture), true);
            start.Load();
          
            specials[START] = start;

            Texture trigger = new Texture(TRIGGER, 0, Path.Combine(basepath, specialTextures, triggerImage), true);
            trigger.Load();
            specials[TRIGGER] = trigger;

        }
        private void loadTextures(string path,bool basic)
        {
            string texturefolder = Path.Combine(path, texturesPath);
            string texturefile = Path.Combine(path, texturesFile);
            try
            {
                using (StreamReader sr = new StreamReader(texturefile))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] line = sr.ReadLine().TrimEnd('\r', '\t', ' ', '\n').Split(' ');
                        int id = Convert.ToInt32(line[0]);
                        textures[id] = new Texture(id, 0, Path.Combine(texturefolder, line[1]),basic);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new MapLoadException("Cannot find tails.txt (" + path + ")", ex);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new MapLoadException("Incorrect format", ex);
            }
            catch (Exception ex)
            {
                throw new MapLoadException("Exception during loading a map", ex);
            }
        }
        public void Save(string name)
        {
            if (!backgraundOn) throw new SavingException("Missing background");
            string tailsFolder = Path.Combine(name, texturesPath);
            string tailsFile = Path.Combine(name, texturesFile);
            string map = Path.Combine(name, "map");
            
            if (!Directory.Exists(name)) Directory.CreateDirectory(name);
            if (!Directory.Exists(tailsFolder)) Directory.CreateDirectory(tailsFolder);
            using (StreamWriter sw = new StreamWriter(map))
            {
                sw.Write(Width);
                sw.Write(" ");
                sw.Write(Height);
                sw.Write("\n");
                sw.Write(startPosition.X*Field.SIZE);
                sw.Write(" ");
                sw.Write(startPosition.Y*Field.SIZE);
                sw.Write("\n");
                int i;
                for (i = 0; i < fields.Length-1; i++)
                {
                    sw.Write(fields[i].Texture.IdTexture);                    
                    sw.Write(" ");
                }
                sw.Write(fields[i].Texture.IdTexture);
            }
            using (StreamWriter sw = new StreamWriter(tailsFile))
            {
                foreach(KeyValuePair<int,Texture> pair in textures)
                {
                    if(!pair.Value.Basic)
                    {
                        string textname = pair.Value.IdTexture.ToString() + ".png";
                        string path = Path.Combine(tailsFolder, textname);
                        sw.WriteLine(pair.Value.IdTexture+" "+textname);
                        pair.Value.Bmp.Save(path);

                    }
                }
            }
            backBit.Save(Path.Combine(name, background));
            EditorEngine.Instance.CreaturesManager.Save(name);
            EditorEngine.Instance.TriggetManager.SaveTriggers(name);
        }

        public void Update()
        {
            if (backgraundOn)
            {
                RectangleF view = EditorEngine.Instance.Camera.GetView();
                DrawBackground(view);
            }
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Field temp = GetField(i, j);
                    temp.Draw(i, j);
                    if (ShowTriggers && temp.triggers != null)
                        temp.Draw(i, j, specials[TRIGGER]);

                }
            }
            if (startPositionSeted)
                GetField(startPosition.X, startPosition.Y).Draw(startPosition.X, startPosition.Y, specials[START]);


        }
        public void DrawBackground(RectangleF view)
        {
            GL.BindTexture(TextureTarget.Texture2D, specials[BACKGROUND].TextureName);
            GL.Begin(BeginMode.Quads);
            GL.PushMatrix();

            GL.TexCoord2(0, 0);
        
            GL.Vertex2(view.Left, view.Bottom);
            GL.TexCoord2(0, 1);
            GL.Vertex2(view.Left, view.Top);
            GL.TexCoord2(1, 1);
            GL.Vertex2(view.Right, view.Top);
            GL.TexCoord2(1, 0);
            GL.Vertex2(view.Right, view.Bottom);

            GL.End();
            GL.PopMatrix();
        }
        public void Update2()
        {
            RectangleF r = EditorEngine.Instance.Camera.GetCameraRectangle();
             Rectangle scaled = new Rectangle(
                (int)r.X / Field.SIZE,
                (int)r.Y / Field.SIZE,
                (int)r.Width / Field.SIZE + 2,
                (int)r.Height / Field.SIZE + 2);
            //if (scaled.X + scaled.Width >= Width) scaled.Width--;
            //if (scaled.Y + scaled.Height >= Height) scaled.Height--;

             
            

            for (int i = scaled.X; i < scaled.X + scaled.Width; i++)
            {
                for (int j = scaled.Y; j < scaled.Y + scaled.Height; j++)
                {
                    GetField(i, j).Draw(i, j);
                }
            }
        }
        public void EditFieldTexture(int x, int y, Texture txt)
        {
            GetField(x, y).Texture = txt;
        }
        public Point FieldPoint(int x, int y)
        {
            int sx = -(int)EditorEngine.Instance.Camera.X + x;
            int sy = -(int)EditorEngine.Instance.Camera.Y + y;
            sx /= Field.SIZE;
            sy /= Field.SIZE;
            return new Point(sx, sy);
        }
        public Rectangle Rectangle(Point location, Point border)
        {
            Point loct = FieldPoint(location.X, location.Y);
            Point bort = FieldPoint(border.X, border.Y);
            Point loc = new Point(Math.Min(loct.X, bort.X), Math.Min(loct.Y, bort.Y));
            Point bor = new Point(Math.Max(loct.X, bort.X), Math.Max(loct.Y, bort.Y));
            return new Rectangle(loc, new Size(bor.X - loc.X, bor.Y - loc.Y));
        }


        public bool InMap(Point point)
        {
            Point p = Utils.PointToOpengl(point);
            p = FieldPoint(p.X,p.Y);
            return p.X < Width && p.Y < Height;
            
        }
    }
}
