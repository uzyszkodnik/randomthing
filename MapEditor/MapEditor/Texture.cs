﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace MapEditor
{
    public class Texture
    {
        private int idTexture;
        private int texture;
        private string path;
        private bool basic;
        private Bitmap bmp;

        public Bitmap Bmp
        {
            get { return bmp; }
            set { bmp = value; }
        }


        public bool IsPathSeted { get { return String.IsNullOrEmpty(path); } }
        public bool Basic
        {
            get { return basic; }
            set { basic = value; }
        }
        

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        

        public int TextureName
        {
            get { return texture; }
            set { texture = value; }
        }
        
        public int IdTexture
        {
            get { return idTexture; }
            set { idTexture = value; }
        }


        public Texture(int id, int text, string path, bool basic)
        {
            this.idTexture = id;
            this.texture = text;
            this.path = path;
            this.basic = basic;

        }

        public void Load()
        {
            string filename = path;
            if (String.IsNullOrEmpty(filename) && bmp==null)
                throw new ArgumentException(filename);

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            if(bmp==null)   bmp = new Bitmap(filename);
            Bitmap temp = Utils.InteligentResize2(bmp);

            BitmapData temp_data = temp.LockBits(new Rectangle(0, 0, temp.Width, temp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, temp_data.Width, temp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, temp_data.Scan0);
            ErrorCode ec = GL.GetError();
            if (ec != ErrorCode.NoError)
            {

                //throw new Exception("LoadTextureError " + ec.ToString() );
            }
            temp.UnlockBits(temp_data);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            this.texture = id;
            
        }
        public void Remove()
        {
            GL.DeleteTexture(TextureName);
        }
    }
}
