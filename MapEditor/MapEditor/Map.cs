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
        }

        private MapLoadingState state;
        private const int DEFAULT_WIDTH = 100;
        private const int DEFAULT_HEIGHT = 100;

        private static string texturesFile = "tiles.txt";
        private static string texturesPath = "tiles";
        private static string maps = "maps";

        private string mapfolder;
        private string basepath;

        Field[] fields;
        Dictionary<int,Texture> textures;
      

        public int Width;
        public int Height;
        public double RealWidth { get { return Width * Field.SIZE; } }
        public double RealHeight { get { return Height * Field.SIZE; } }
        private string name;
        public Map()
            :this(DEFAULT_WIDTH,DEFAULT_HEIGHT)
        {
            
        }
        public Map(int width, int height)
        {/*
            textures = new List<int>();
            loadTextures();
            fields = new Field[width][];
            for (int i = 0; i < width; i++)
            {
                fields[i] = new Field[height];
                //
                for (int j = 0; j < height; j++)
                {
                    fields[i][j] = new Field(i == 2 ? textures[0]:textures[1], 0);
                }
                //
            }*/
        }
        public Map(string name)
        {
            this.mapfolder = Path.Combine(Application.StartupPath,maps,name);
            this.basepath = Application.StartupPath;
            textures = new Dictionary<int, Texture>();
            this.name = Path.GetFileName(mapfolder);
            LoadTextures();
            LoadTiles();


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
        private Field GetField(int x, int y)
        {
            return fields[y * Width + x];
        }
        private void SetField(int x, int y, Field f)
        {
            fields[y * Width + x] = f;
        }
        public void LoadTextures()
        {
            loadTextures(basepath,true);
            loadTextures(mapfolder,false);
            foreach (KeyValuePair<int, Texture> pair in textures)
            {
                pair.Value.Load();
            }
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
            string tailsFolder = Path.Combine(name, texturesPath);
            string tailsFile = Path.Combine(name, texturesFile);
            string triggers = Path.Combine(name, "triggers");
            string map = Path.Combine(name, "map");
            if (!Directory.Exists(name)) Directory.CreateDirectory(name);
            if (!Directory.Exists(tailsFolder)) Directory.CreateDirectory(tailsFolder);
            if (!Directory.Exists(triggers)) Directory.CreateDirectory(triggers);
            using (StreamWriter sw = new StreamWriter(map))
            {
                sw.Write(Width);
                sw.Write(" ");
                sw.Write(Height);
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
            
        }

        public void Update()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    GetField(i, j).Draw(i, j);
                }
            }
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

    }
}
