﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    public class LostGameTrigger
    {
        PositionAABBCueLauncher juegoTerminado;
        bool fin = false;
        float duracion = 5;
        Cue obi_triste;
        FullScreenElement failed;
        public LostGameTrigger(ITarget target,TGCVector3 position)
        {
            juegoTerminado = new PositionAABBCueLauncher(target, position, new TGCVector3(1000,1000,20));
            obi_triste = new Cue(null, "Bitmaps\\Game_Lost.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, duracion);
            failed = new FullScreenElement("Bitmaps\\Failed.png", SoundManager.SONIDOS.NO_SOUND, duracion);
        }
        public void Update()
        {
            if (!fin &&
                (juegoTerminado.IsReady() || VariablesGlobales.vidas == 0)
                    && !VariablesGlobales.MODO_DIOS)
            {
                fin = true;
            }
        }
        public void Render()
        {
            if (fin)
            {
                RenderLost();
                duracion -= VariablesGlobales.elapsedTime;
            }
        }
        public bool GameFinished()
        {
            return fin;
        }
        public bool Terminado()
        {
            return duracion < 0;
        }

        public void RenderLost()//@@agregar postprocesado q se oscurezca la pantalla
        {
            obi_triste.Update();
            obi_triste.Render();
            failed.Render();
        }
    }
}
