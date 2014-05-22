using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat;
using Kethane;
using UnityEngine;

namespace SCANsatKethane
{
    public class SCANsatkethane: MonoBehaviour
    {
        private CelestialBody body;


        public void Start()
        {
        }

        public void Update()
        {
            setBody (FlightGlobals.currentMainBody);
        }

        private void setBody(CelestialBody b) {
            if (body == b)
                return;
            body = b;
        }

        private void rebuildResourceArray()
        {
            string resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
            int kethaneStep = 0;
            for (int ilon = 0; ilon < 360; ilon++)
            {
                Cell cell = getKethaneCell(ilon, kethaneStep);
                if (KethaneData.Current.Scans[resource][body.name][cell])
                {
                    updateResourceArray(ilon, kethaneStep, SCANcontroller.controller.OverlayResourceType(resource));
                    ICellResource deposit = KethaneData.Current.GetCellDeposit(resource, body, cell);
                    if (deposit != null) updateResourceValue(ilon, kethaneStep, deposit.Quantity);
                    else updateResourceValue(ilon, kethaneStep, 0d);
                }
                kethaneStep++;
            }
        }

        private Cell getKethaneCell(int ilon, int ilat)
        {
            Vector3 pos = body.GetWorldSurfacePosition((double)ilat, (double)ilon, 50000);
            Vector3 Wpos = body.transform.InverseTransformPoint(pos);
            return Kethane.Cell.Containing(Wpos, 5);
        }

        private void updateResourceArray (int lon, int lat, SCANdata.SCANResourceType type)
        {
            SCANdata data = SCANcontroller.controller.getData(body);
            data.resourceCoverage[lon, lat] = (byte)type;
        }

        private void updateResourceValue (int lon, int lat, double value)
        {
            SCANdata data = SCANcontroller.controller.getData(body);
            data.kethanemap[lon, lat] = (float)value;
        }
    }
}
