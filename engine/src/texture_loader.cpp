#include "texture_loader.h"
#include <string>
#include <memory>
#include <SDL/SDL.h>
#include <GL/gl.h>

Texture::Texture(std::string path){
    GLenum texture_format;
    GLint  nOfColors;
    SDL_Surface* surface;
 
    if (  surface = SDL_LoadBMP(path.c_str()) ) { 
    
        // Check that the image's width is a power of 2
        if ( (surface->w & (surface->w - 1)) != 0 ) {
            printf("warning: image.bmp's width is not a power of 2\n");
        }
    
        // Also check if the height is a power of 2
        if ( (surface->h & (surface->h - 1)) != 0 ) {
            printf("warning: image.bmp's height is not a power of 2\n");
        }
    
            // get the number of channels in the SDL surface
            nOfColors = surface->format->BytesPerPixel;
            if (nOfColors == 4)     // contains an alpha channel
            {
                    if (surface->format->Rmask == 0x000000ff)
                            texture_format = GL_RGBA;
                    else
                            texture_format = GL_BGRA;
            } else if (nOfColors == 3)     // no alpha channel
            {
                    if (surface->format->Rmask == 0x000000ff)
                            texture_format = GL_RGB;
                    else
                            texture_format = GL_BGR;
            } else {
                    printf("warning: the image is not truecolor..  this will probably break\n");
                    // this error should not go unhandled
            }
    
        // Have OpenGL generate a texture object handle for us
        glGenTextures( 1, &texture );
    
        // Bind the texture object
        glBindTexture( GL_TEXTURE_2D, texture );
    
        // Set the texture's stretching properties
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
    
        // Edit the texture object's image data using the information SDL_Surface gives us
        glTexImage2D( GL_TEXTURE_2D, 0, nOfColors, surface->w, surface->h, 0,
                        texture_format, GL_UNSIGNED_BYTE, surface->pixels );
    } 
    else {
        printf("SDL could not load image.bmp: %s\n", SDL_GetError());
        SDL_Quit();
        exit(1);
    }    

    width = surface->w;
    height = surface -> h; 

// Free the SDL_Surface only if it was successfully created
    if ( surface ) { 
        SDL_FreeSurface( surface );
    }
}
inline void Texture::set(){
    glBindTexture(GL_TEXTURE_2D, texture);
}

const  std::shared_ptr<Texture>& TextureLoader::operator[](std::string name){

    if(cache.find(name) == cache.end()){
        std::shared_ptr<Texture>  p(new Texture(name)); 
        cache[name] = p ;
    }
    return cache[name];

}
