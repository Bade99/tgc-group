﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    public class PhysicsEngine
    {
        protected CollisionWorld collisionWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        //Inventario
        private Dictionary<int, CollisionObject> listaMisilesEnemigo;
        private Dictionary<int, CollisionObject> listaMisilesXwing;
        private Dictionary<int, Torreta> listaTorretas;
        private Dictionary<int, Obstaculo> listaObstaculos;
        //Colisiones
        private List<int> listaIdMisilesQueColisionaronConXwing;
        private HashSet<Colision> listaColisionesTorretaMisil;
        private HashSet<Colision> listaColisionesObstaculoMisil;
        //Ids utilizados para reconocer objetos
        private int misilXwingCount = 1000;
        private int misilEnemigoCount = 1001;
        private int torretaIdCount = 3;
        private int obstaculoIdCount = 501;
        private int xwingEnemigoIdCount = 2;
        private static readonly int ID_XWING = 1;
        private static readonly int ID_PARED_OBSTACULO = -2;

        private readonly static float epsilonContact = 1f;
        CollisionObject main_character;

        public PhysicsEngine()
        {
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            BroadphaseInterface overlappingPairCache = new DbvtBroadphase();
            collisionWorld = new CollisionWorld(dispatcher, overlappingPairCache, collisionConfiguration);
            listaMisilesEnemigo = new Dictionary<int, CollisionObject>();
            listaMisilesXwing = new Dictionary<int, CollisionObject>();
            listaTorretas = new Dictionary<int, Torreta>();
            listaObstaculos = new Dictionary<int, Obstaculo>();
            listaIdMisilesQueColisionaronConXwing = new List<int>();
            listaColisionesTorretaMisil = new HashSet<Colision>(new ColisionCompare());
            listaColisionesObstaculoMisil = new HashSet<Colision>(new ColisionCompare());
            collisionWorld.DebugDrawer = new Debug_Draw_Bullet();
            collisionWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
        }

        public CollisionObject AgregarMisilEnemigo(TGCVector3 size)
        {
            CollisionObject misilActual = AgregarMisil(size);
            listaMisilesEnemigo.Add(misilEnemigoCount, misilActual);
            misilActual.UserIndex = misilEnemigoCount;
            misilEnemigoCount += 2;
            return misilActual;
        }
        public CollisionObject AgregarMisilXwing(TGCVector3 size)
        {
            CollisionObject misilActual = AgregarMisil(size);
            misilActual.UserIndex = misilXwingCount;
            listaMisilesXwing.Add(misilXwingCount, misilActual);
            misilXwingCount += 2;
            return misilActual;
        }

        public CollisionObject AgregarTorreta(Torreta torreta, TGCVector3 size)
        {
            CollisionObject torretaCollision = CrearCollisionObject(size);
            collisionWorld.AddCollisionObject(torretaCollision);
            listaTorretas.Add(torretaIdCount, torreta);
            torretaCollision.UserIndex = torretaIdCount;
            torretaIdCount += 2;
            return torretaCollision;
        }

        public CollisionObject AgregarObstaculo(Obstaculo obstaculo, TGCVector3 size)
        {
            CollisionObject obstaculoCollision = CrearCollisionObject(size);
            collisionWorld.AddCollisionObject(obstaculoCollision);
            listaObstaculos.Add(obstaculoIdCount, obstaculo);
            obstaculoCollision.UserIndex = obstaculoIdCount;
            obstaculoIdCount += 2;
            return obstaculoCollision;
        }

        private CollisionObject AgregarMisil(TGCVector3 size)
        {
            CollisionObject misil = CrearCollisionObject(size);
            collisionWorld.AddCollisionObject(misil);
            return misil;
        }

        private CollisionObject AgregarEscenario(TGCVector3 size)
        {
            CollisionObject pared = CrearCollisionObject(size);
            collisionWorld.AddCollisionObject(pared);
            return pared;
        }
        public CollisionObject AgregarParedObstaculo(TGCVector3 size)
        {
            CollisionObject piso = AgregarEscenario(size);
            piso.UserIndex = ID_PARED_OBSTACULO;
            return piso;
        }

        public CollisionObject AgregarXwing(TGCVector3 size)
        {
            main_character = CrearCollisionObject(size);
            main_character.UserIndex = 1;
            collisionWorld.AddCollisionObject(main_character);
            return main_character;
        }
        public void EliminarObjeto(CollisionObject objeto)
        {
            collisionWorld.RemoveCollisionObject(objeto);
            objeto.Dispose();
        }

        public void Update()
        {
            collisionWorld.PerformDiscreteCollisionDetection();
            int numManifolds = collisionWorld.Dispatcher.NumManifolds;
            for (int i = 0; i < numManifolds; i++)
            {
                PersistentManifold contactManifold = collisionWorld.Dispatcher.GetManifoldByIndexInternal(i);
                CollisionObject obA = contactManifold.Body0;
                CollisionObject obB = contactManifold.Body1;
                int misilId = obB.UserIndex;
                int objetoId = obA.UserIndex;
                if (objetoId == ID_XWING && EsMisilEnemigo(misilId) 
                    && !Xwing.ESTADO_BARREL.BARRELROLL.Equals(VariablesGlobales.xwing.getEstadoBarrel())
                    && !listaIdMisilesQueColisionaronConXwing.Contains(misilId))
                {
                    XwingCollision(contactManifold, misilId);
                }
                if (EsTorreta(objetoId) && EsMisilXWing(misilId) &&
                    !(listaColisionesTorretaMisil.Contains(new Colision(objetoId, misilId))))
                {
                    TorretaCollision(contactManifold, misilId, objetoId);
                }
                if (EsObstaculo(objetoId) && EsMisilXWing(misilId) &&
                    !(listaColisionesObstaculoMisil.Contains(new Colision(objetoId, misilId))))
                {
                    ObstaculoCollision(contactManifold, misilId, objetoId);
                }
                if (objetoId == ID_XWING && (misilId == ID_PARED_OBSTACULO || EsTorreta(misilId)))
                {
                    VariablesGlobales.xwing.ChocarPared();
                }
                collisionWorld.Dispatcher.ClearManifold(contactManifold);
            }
        }


        private bool EsMisilEnemigo(int misilId)
        {
            return (misilId % 2 == 1 && misilId > 1000);
        }
        private bool EsMisilXWing(int misilId)
        {
            if((misilId % 2 == 0) && misilId > 999)
            {
                return true;
            }
            return false;
        }
        private bool EsTorreta(int misilId)
        {
            return (misilId % 2 == 1) && misilId < 500;
        }
        private bool EsObstaculo(int misilId)
        {
            if((misilId % 2 == 1) && misilId > 500 && misilId < 1000)
            {
                return true;
            }
            return false;
        }
        private bool EsXwingEnemigo(int misilId)
        {
            return (misilId % 2 == 0) && misilId < 999;
        }
        private void XwingCollision(PersistentManifold contactManifold, int misilId)
        {
            int numContacts = contactManifold.NumContacts;
            for (int j = 0; j < numContacts && !listaIdMisilesQueColisionaronConXwing.Contains(misilId); j++)
            {

                ManifoldPoint pt = contactManifold.GetContactPoint(j);
                double ptdist = pt.Distance;
                if (Math.Abs(ptdist) < epsilonContact)
                {
                    listaIdMisilesQueColisionaronConXwing.Add(misilId);
                    collisionWorld.RemoveCollisionObject(listaMisilesEnemigo[misilId]);
                    listaMisilesEnemigo.Remove(misilId);
                    VariablesGlobales.RestarVida();
                }
            }
        }
        private void TorretaCollision(PersistentManifold contactManifold, int misilId, int objetoId)
        {
            int numContacts = contactManifold.NumContacts;
            for (int j = 0; j < numContacts; j++)
            {
                ManifoldPoint pt = contactManifold.GetContactPoint(j);
                double ptdist = pt.Distance;
                Console.WriteLine(ptdist);
                if (Math.Abs(ptdist) < 6)
                {
                    listaColisionesTorretaMisil.Add(new Colision(objetoId, misilId));
                    listaTorretas[objetoId].DisminuirVida();
                }
            }

        }

        private void ObstaculoCollision(PersistentManifold contactManifold, int misilId, int objetoId)
        {
            int numContacts = contactManifold.NumContacts;
            for (int j = 0; j < numContacts; j++)
            {
                ManifoldPoint pt = contactManifold.GetContactPoint(j);
                double ptdist = pt.Distance;
                if (Math.Abs(ptdist) < epsilonContact)
                {
                    listaColisionesObstaculoMisil.Add(new Colision(objetoId, misilId));
                    listaObstaculos[objetoId].Destruir();
                }
            }

        }

        public void Render(TgcD3dInput input)
        {
            if (VariablesGlobales.debugMode)
            {
                collisionWorld.DebugDrawWorld();
            }
        }

        public void Dispose()
        {
            collisionWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
        }
        private CollisionObject CrearCollisionObject(TGCVector3 size)
        {
            CollisionObject collisionObject = new CollisionObject();
            BoxShape box2DShape = new BoxShape(CommonHelper.VectorXEscalar(size, 0.5f).ToBulletVector3());
            collisionObject.CollisionShape = box2DShape;
            collisionObject.SetCustomDebugColor(new Vector3(1, 1, 0));
            return collisionObject;
        }
        private class Colision {
            public int oA;
            public int oB;
            public Colision(int oA, int oB)
            {
                this.oA = oA;
                this.oB = oB;
            }
        }
        private class ColisionCompare : IEqualityComparer<Colision>
        {
            bool IEqualityComparer<Colision>.Equals(Colision x, Colision y)
            {
                return x.oA == y.oA && x.oB == y.oB;
            }

            int IEqualityComparer<Colision>.GetHashCode(Colision obj)
            {
                return obj.oA * 100 + obj.oB;
            }
        }
        private CollisionObject CreateCollisionFromTgcMesh(TgcMesh mesh)
        {
            var vertexCoords = mesh.getVertexPositions();

            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < vertexCoords.Length; i = i + 3)
            {
                triangleMesh.AddTriangle(vertexCoords[i].ToBulletVector3(), vertexCoords[i + 1].ToBulletVector3(), vertexCoords[i + 2].ToBulletVector3());
            }

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            var bulletShape = new BvhTriangleMeshShape(triangleMesh, false);

            CollisionObject collisionObject = new CollisionObject();
            collisionObject.CollisionShape = bulletShape;
            return collisionObject;
        }
    }
}
