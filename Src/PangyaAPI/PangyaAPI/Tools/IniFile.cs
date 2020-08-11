#region Reference
//* A simple but complete ini-read-library.
//* Copyright © 15peaces 2012 - 2017
//* Code Updated by: LuisMK 2020
//* Link: https://github.com/15peaces/PangServ/blob/master/src/common/inilib.cs
#endregion
namespace PangyaAPI.Tools
{
    using System;
    using System.IO;
    public class IniFile : IDisposable
    {
        #region Private Fields
        private readonly string _file;
        private bool disposedValue;
        private readonly string fn;
        private readonly string[] lines;
        #endregion

        #region Construtor
        // Constructor
        public IniFile(string filename)
        {
            _file = filename;
            filename = AppDomain.CurrentDomain.BaseDirectory + filename;
            try
            {
                using (StreamReader stream = new StreamReader(filename))
                {
                    fn = _file;
                    lines = stream.ReadToEnd().Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
                }
            }
            catch (Exception e)
            {
                WriteConsole.Error("The file could not be read:");
                WriteConsole.Error(e.Message);
                return;
            }
        }
        #endregion
       
        #region Private Methods
        // Get group-range       
        int[] GroupPos(string group = "")
        {
            if (group == "")
            {
                return new[] { 0, 0 };// No Group.
            }

            string lowerline;
            int[] ret = new[] { -1, -1 };
            for (int i = 0; i < lines.Length; i++)
            {
                lowerline = lines[i].ToLower();

                if (ret[0] < 0)
                {
                    if (lowerline.Contains("[" + group.ToLower() + "]"))
                    {
                        ret[0] = i; // Group found.
                    }

                }
                else
                {
                    if (lowerline.StartsWith("[") || i == lines.Length - 1) // next group or end of file.
                    {
                        ret[1] = --i; // End of group found.
                        return ret;
                    }
                }
            }
            WriteConsole.Error("Unable to find Group '" + group + "' in configuration file '" + fn + "'.");
            return ret; // Group not found.
        }

        object GetValue(string section, string key, object _default, int min = 0, int max = 65535)
        {
            int[] group_index = GroupPos(section);

            if (group_index[0] < 0 || group_index[1] > lines.Length)
            {
                return _default;
            }

            object[] tarr = null;
            for (int i = group_index[0]; i < group_index[1]; i++)
            {
                if (lines[i].StartsWith(key))
                {
                    tarr = lines[i].Split(new[] { "=" }, StringSplitOptions.None);
                    break;
                }
            }

            object ret;
            if (tarr == null)
            {
                ret = _default;
            }
            else
            {
                ret = tarr[1];
            }

            // Assuming integer value and checking min / max values.
            if (min != int.MinValue || max != int.MaxValue)
            {
                int iret = Convert.ToInt32(ret.ToString().Length);
                if (iret < min || iret > max)
                {
                    ret = _default;
                }
            }
            return ret;
        }
        #endregion

        #region Public Methods
        public string ReadString(string section, string key, string _default = "")
        {
            return Convert.ToString(GetValue(section, key, _default, 0, 65535));
        }

        public int ReadInt32(string section, string key, int _default = 0)
        {
            return Convert.ToInt32(GetValue(section, key, _default, 0, 65535));
        }

        public uint ReadUInt32(string section, string key, uint _default = 0)
        {
            return Convert.ToUInt32(GetValue(section, key, _default, 0, 65535));
        }

        public long ReadInt64(string section, string key, long _default = 0)
        {
            return Convert.ToInt64(GetValue(section, key, _default, 0, 65535));
        }
    
        public ulong ReadUInt64(string section, string key, ulong _default = 0)
        {
            return Convert.ToUInt64(GetValue(section, key, _default, 0, 65535));
        }

        public bool ReadBool(string section, string key, bool _default = false)
        {
            return Convert.ToBoolean(GetValue(section, key, _default, 0, 65535));
        }

        public byte ReadByte(string section, string key, byte _default = 0)
        {
            return Convert.ToByte(GetValue(section, key, _default, 0, 65535));
        }

        public ushort ReadUInt16(string section, string key, ushort _default =0)
        {
            return Convert.ToUInt16(GetValue(section, key, _default, 0, 65535));
        }

        public short ReadInt16(string section, string key, short _default = 0)
        {
            return Convert.ToInt16(GetValue(section, key, _default, 0, 65535));
        }

        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Tarefa pendente: descartar o estado gerenciado (objetos gerenciados)
                }

                // Tarefa pendente: liberar recursos não gerenciados (objetos não gerenciados) e substituir o finalizador
                // Tarefa pendente: definir campos grandes como nulos
                disposedValue = true;
            }
        }

        // Tarefa pendente: substituir o finalizador somente se 'Dispose(bool disposing)' tiver o código para liberar recursos não gerenciados
        ~IniFile()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
