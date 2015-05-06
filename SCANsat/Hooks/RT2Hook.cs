///////////////////////////////////////////////////////////////////////////////
//
//    RemoteTech Hook code by Cilph from KOS Mod
//    Much thanks to him for his help on this part ;)
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SCANsat.Hooks
{
    public static class RT2Hook
    {
        public const String RemoteTechAssembly = "RemoteTech";
        public const String RemoteTechApi = "RemoteTech.API.API";

        private static bool mHookFail;
        private static IRemoteTechAPIv1 mInstance;

        public static IRemoteTechAPIv1 Instance
        {
            get
            {
                if (mHookFail) return null;
                mInstance = mInstance ?? InitializeAPI();
                if (mInstance == null) mHookFail = true;
                return mInstance;
            }
        }

        private static IRemoteTechAPIv1 InitializeAPI()
        {
            var loadedAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name.Equals(RemoteTechAssembly));
            if (loadedAssembly == null) return null;

            var type = loadedAssembly.assembly.GetTypes().FirstOrDefault(t => t.FullName.Equals(RemoteTechApi));
            if (type == null) return null;

            var methods = type.GetMethods();
            var api = new RTInterfaceImplementation();

            try
            {
                foreach (var property in api.GetType().GetProperties())
                {
                    var method = methods.FirstOrDefault(m => { Debug.Log(m.Name); return m.Name.Equals(property.Name); });
                    if (method == null) throw new ArgumentNullException(property.Name);
                    var del = Delegate.CreateDelegate(property.PropertyType, type, method.Name);
                    property.SetValue(api, del, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("[SCANlogger] Error creating RemoteTech interface: " + e);
                return null;
            }

            Debug.Log("[SCANlogger] RemoteTech interface successfully created.");
            return api;
        }
    }

    internal class RTInterfaceImplementation : IRemoteTechAPIv1
    {
        public Func<Guid, bool> HasFlightComputer { get; internal set; }
        public Action<Guid, Action<FlightCtrlState>> AddSanctionedPilot { get; internal set; }
        public Action<Guid, Action<FlightCtrlState>> RemoveSanctionedPilot { get; internal set; }
        public Func<Guid, bool> HasAnyConnection { get; internal set; }
        public Func<Guid, bool> HasConnectionToKSC { get; internal set; }
        public Func<Guid, double> GetShortestSignalDelay { get; internal set; }
        public Func<Guid, double> GetSignalDelayToKSC { get; internal set; }
        public Func<Guid, Guid, double> GetSignalDelayToSatellite { get; internal set; }
    }

    public interface IRemoteTechAPIv1
    {
        Func<Guid, bool> HasFlightComputer { get; }
        Action<Guid, Action<FlightCtrlState>> AddSanctionedPilot { get; }
        Action<Guid, Action<FlightCtrlState>> RemoveSanctionedPilot { get; }
        Func<Guid, bool> HasAnyConnection { get; }
        Func<Guid, bool> HasConnectionToKSC { get; }
        Func<Guid, double> GetShortestSignalDelay { get; }
        Func<Guid, double> GetSignalDelayToKSC { get; }
        Func<Guid, Guid, double> GetSignalDelayToSatellite { get; }
    }
}