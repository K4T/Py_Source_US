using PangyaAPI.BinaryModels;
using Py_Game.Data;
namespace Py_Game.Client.Inventory.Data
{
    public struct Action
    {
        public uint Animate;
        public ushort Unknown1;
        public uint Posture;
        public Point3D Vector { get; set; }
        public string Animation;
        public Action Clear()
        {
            Animate = 0;
            Unknown1 = 3490;
            Posture = 0;
            Vector = new Point3D();
            Animation = "";
            return this;
        }

        public byte[] ToArray()
        {
            PangyaBinaryWriter Packet;
            Packet = new PangyaBinaryWriter();
            try
            {
                Packet.WriteUInt32(Animate);
                Packet.WriteUInt16(Unknown1);
                Packet.WriteUInt32(Posture);
                Packet.WriteStruct(Vector);
                return Packet.GetBytes();
            }
            finally
            {
                Packet.Dispose();
            }
        }

        public float Distance(Point3D Position)
        {
            return Vector.Distance(Position);
        }
    }
}

