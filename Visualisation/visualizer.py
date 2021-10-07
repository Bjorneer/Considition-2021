# code from https://www.youtube.com/watch?v=Hqg4qePJV2U
# Pyglet OpenGL tutorial by DLC ENERGY
# As taught by Aiden Ward

import pyglet
from pyglet.gl import *
from pyglet.window import key
import math
import numpy as np
import pandas as pd
import random

f = pd.read_csv('./Visualisation/visualization.txt')
size = f.iloc[0]
boxes = f.iloc[2:]
maxBox = f.iloc[1]


class Model:

    def get_tex(self,file):
        tex = pyglet.image.load(file).get_texture()
        glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST)
        glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
        return pyglet.graphics.TextureGroup(tex)

    def add_block(self,x,y,z, length, widht, height, block, weight, order):
        z, y = y, z
        height, widht = widht, height
        X, Y, Z = x+length, y+widht, z+height
        
        black = ('c4B', [0,0,0,0,0,0,0,0])
        orderClr = []
        if block:
            if order == 0:
                if weight == 0:
                    clr = [178,102,255,255]
                elif weight == 1:
                    clr = [153,51,255,255]
                else:
                    clr = [127,0,255,255]
            elif order == 1:
                if weight == 0:
                    clr = [102,178,255,255]
                elif weight == 1:
                    clr = [51,153,255,255]
                else:
                    clr = [0,128,255,255]
            elif order == 2:
                if weight == 0:
                    clr = [51,255,51,255]
                elif weight == 1:
                    clr = [0,255,0,255]
                else:
                    clr = [0,204,0,255]
            elif order == 3:
                if weight == 0:
                    clr = [255,255,51,255]
                elif weight == 1:
                    clr = [255,255,0,255]
                else:
                    clr = [204,204,0,255]
            elif order == 4:
                if weight == 0:
                    clr = [255,51,51,255]
                elif weight == 1:
                    clr = [255,0,0,255]
                else:
                    clr = [204,0,0,255]
            #clr = [random.randint(0, 255), random.randint(0, 255), random.randint(0, 255), 255]

            tex_coords = ('c4B', clr * 4)
            self.batch.add(4, GL_QUADS, None,   ('v3f', (X, y, z,  x, y, z,  x, Y, z,  X, Y, z)), tex_coords)
            self.batch.add(4, GL_QUADS, None,   ('v3f', (x, y, Z,  X, y, Z,  X, Y, Z,  x, Y, Z)), tex_coords) 

            self.batch.add(4, GL_QUADS, None,   ('v3f', (x, y, z,  x, y, Z,  x, Y, Z,  x, Y, z)), tex_coords)
            self.batch.add(4, GL_QUADS, None,   ('v3f', (X, y, Z,  X, y, z,  X, Y, z,  X, Y, Z)), tex_coords)

            self.batch.add(4, GL_QUADS, None, ('v3f', (x, y, z,  X, y, z,  X, y, Z,  x, y, Z)), tex_coords)
            self.batch.add(4, GL_QUADS, None,    ('v3f', (x, Y, Z,  X, Y, Z,  X, Y, z,  x, Y, z)), tex_coords) 

        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, y, z,  X, y, z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, y, z,  x, Y, z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, y, z,  x, y, Z)), black)

        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (X, y, z,  X, Y, z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (X, y, z,  X, y, Z)), black)

        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, Y, z,  x, Y, Z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, Y, z,  X, Y, z)), black)

        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, y, Z,  x, Y, Z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (x, y, Z,  X, y, Z)), black)

        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (X, Y, Z,  x, Y, Z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (X, Y, Z,  X, y, Z)), black)
        self.batch.add(2, pyglet.gl.GL_LINES, None, ('v3f', (X, Y, Z,  X, Y, z)), black)


    def __init__(self):
        self.batch = pyglet.graphics.Batch()

        self.add_block(size['x'], size['y'], size['z'], size['length'], size['width'], size['height'], False, 0, 0)
        for idx, element in boxes.iterrows():
            self.add_block(element['x'], element['y'], element['z'], element['length'], element['width'], element['height'], True, element['weight'], element['order'])


    def draw(self):
        self.batch.draw()

class Player:
    def __init__(self, pos=(0, 0, 0), rot=(0, 0)):
        self.pos = list(pos)
        self.rot = list(rot)

    def mouse_motion(self, dx, dy):
        dx/= 8
        dy/= 8
        self.rot[0] += dy
        self.rot[1] -= dx
        if self.rot[0]>90:
            self.rot[0] = 90
        elif self.rot[0] < -90:
            self.rot[0] = -90

    def update(self,dt,keys):
        sens = 1
        s = dt*100
        rotY = -self.rot[1]/180*math.pi
        dx, dz = s*math.sin(rotY), math.cos(rotY)
        if keys[key.W]:
            self.pos[0] += dx*sens
            self.pos[2] -= dz*sens
        if keys[key.S]:
            self.pos[0] -= dx*sens
            self.pos[2] += dz*sens
        if keys[key.A]:
            self.pos[0] -= dz*sens
            self.pos[2] -= dx*sens
        if keys[key.D]:
            self.pos[0] += dz*sens
            self.pos[2] += dx*sens
        if keys[key.SPACE]:
            self.pos[1] += s
        if keys[key.LSHIFT]:
            self.pos[1] -= s

class Window(pyglet.window.Window):

    def push(self,pos,rot):
        glPushMatrix()
        rot = self.player.rot
        pos = self.player.pos
        glRotatef(-rot[0],1,0,0)
        glRotatef(-rot[1],0,1,0)
        glTranslatef(-pos[0], -pos[1], -pos[2])

    def Projection(self):
        glMatrixMode(GL_PROJECTION)
        glLoadIdentity()

    def Model(self):
        glMatrixMode(GL_MODELVIEW)
        glLoadIdentity()

    def set2d(self):
        self.Projection()
        gluPerspective(0, self.width, 0, self.height)
        self.Model()

    def set3d(self):
        self.Projection()
        gluPerspective(100, self.width/self.height, 10, 1000)
        self.Model()

    def setLock(self, state):
        self.lock = state
        self.set_exclusive_mouse(state)

    lock = False
    mouse_lock = property(lambda self:self.lock, setLock)

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.set_minimum_size(300,200)
        self.keys = key.KeyStateHandler()
        self.push_handlers(self.keys)
        pyglet.clock.schedule(self.update)

        self.model = Model()
        self.player = Player((0.5,1.5,1.5),(-30,0))

    def on_mouse_motion(self,x,y,dx,dy):
        if self.mouse_lock: self.player.mouse_motion(dx,dy)

    def on_key_press(self, KEY, _MOD):
        if KEY == key.ESCAPE:
            self.close()
        elif KEY == key.E:
            self.mouse_lock = not self.mouse_lock

    def update(self, dt):
        self.player.update(dt, self.keys)

    def on_draw(self):
        self.clear()
        self.set3d()
        self.push(self.player.pos,self.player.rot)
        self.model.draw()
        glPopMatrix()

if __name__ == '__main__':
    window = Window(width=400, height=300, caption='Visualizer',resizable=True)
    glClearColor(0.5,0.7,1,1)
    glEnable(GL_DEPTH_TEST)
    #glEnable(GL_CULL_FACE)
    pyglet.app.run()