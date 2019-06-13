﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    public static class VariablesGlobales
    {
        public static string mediaDir;
        public static string shadersDir;
        public static TgcSceneLoader loader;
        public static Microsoft.DirectX.DirectSound.Device soundDevice;
        public static float elapsedTime;
        public static SoundManager managerSonido;
        public static PhysicsEngine physicsEngine;
        public static TemporaryElementManager managerElementosTemporales;
        public static EnemyManager managerEnemigos;
        public static ShaderManager shaderManager;
        public static bool BULLET=true;
        public static Xwing xwing;
        public static bool POSTPROCESS = true;
        public static PostProcess postProcess;
        public static bool SOUND=true;
        public static float time = 5;//para testeos con temporizador
        public static bool SHADERS = true;
        public static Effect shader;//effect que todos los objetos a renderizar deben usar
        public static bool DameLuz = true;
        public static EndgameManager endgameManager;
        public static TGCVector2 cues_relative_position;
        public static float cues_relative_scale;
        public static int vidas = 3;
        public static bool MODO_DIOS = false;
    }
}
