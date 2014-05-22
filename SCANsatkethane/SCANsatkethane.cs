using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat;
using Kethane;
using UnityEngine;

namespace SCANsatKethane
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SCANsatkethane: MonoBehaviour
    {
        private CelestialBody body;
        private string resource;
        private bool rebuild;
        private bool reset;
        private bool initialized = false;
        private int rebuildStep;
        private int rebuildValueStep;
        private int rebuildFrame = 0;
        private bool printer = true;
        private bool printera = true;

        public void Start()
        {
            print("[Kethane Watcher] Starting Up");
            GameEvents.onFlightReady.Add(initialize);
        }

        public void OnDestroy()
        {
            GameEvents.onFlightReady.Remove(initialize);
        }

        public void Update()
        {
            if (initialized) {
                if (rebuildStep < 180) {
                    rebuildResourceArray();
                }
                if (rebuildValueStep < 180) {
                    rebuildResourceValue(SCANcontroller.controller.OverlayResourceType(resource));
                }
                if (SCANcontroller.controller.resourceOverlayType == 1) {
                    setBody (FlightGlobals.currentMainBody);
                    setResource (SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection]);
                    setRebuild (SCANcontroller.controller.kethaneRebuild);
                    setReset (SCANcontroller.controller.kethaneReset);
                }
            }
        }

        private void initialize()
        {
            rebuild = SCANcontroller.controller.kethaneRebuild;
            print("[Kethane Watcher] Initialized");
            rebuildStep = 180;
            rebuildValueStep = 180;
            initialized = true;
        }

        private void setBody(CelestialBody b) { //Watcher to check for Celestial Body changes
            if (body == b) return;
            body = b;
            resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
            reset = SCANcontroller.controller.kethaneReset;
            SCANcontroller.controller.map_ResourceOverlay = false; //Force the overlay off to allow array to rebuild
            SCANdata data = SCANcontroller.controller.getData(body);
            data.kethaneValueMap = new float[360, 180];
            rebuildStep = 0;
            rebuildFrame = 0;
            SCANcontroller.controller.kethaneBusy = true;
            //rebuildResourceArray(); //Update coverage map and reset resource value array
        }

        private void setResource(string s) { //Watcher to check for the user switching to different Kethane resources
            if (s == resource) return;
            resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
            reset = SCANcontroller.controller.kethaneReset;
            SCANcontroller.controller.map_ResourceOverlay = false; //Force the overlay off to allow array to rebuild
            SCANdata data = SCANcontroller.controller.getData (body);
            data.kethaneValueMap = new float[360, 180]; //Rebuild the resource value array
            rebuildStep = 0;
            rebuildFrame = 0;
            SCANcontroller.controller.kethaneBusy = true;
            //rebuildResourceArray();
        }

        private void setRebuild(bool b) { //Watcher to check for "Rebuild" button pushes from UI
            if (b == rebuild) return;
            rebuild = b;
            updateKethaneData (SCANcontroller.controller.OverlayResourceType(resource)); //Update the Kethane database after background scanning
        }

        private void setReset (bool b) { //Watcher to check for big map reset
            if (b == reset) return;
            reset = b;
            rebuildValueStep = 0;
            rebuildResourceValue (SCANcontroller.controller.OverlayResourceType(resource));
        }

        //Update the Kethane database - used after background scanning
        private void updateKethaneData (SCANdata.SCANResourceType type)
        {
            print("[Kethane Watcher] Updating Kethane Database");
            SCANdata data = SCANcontroller.controller.getData (body);
            for (int ilat = 0; ilat < 180; ilat++)
            {
                for (int ilon = 0; ilon < 360; ilon++)
                {
                    if (data.isCoveredResource (ilon, ilat, type))
                    {
                        Cell cell = getKethaneCell (ilon, ilat);
                        if (!KethaneData.Current.Scans[resource][body.name][cell])
                            KethaneData.Current.Scans[resource][body.name][cell] = true;
                    }
                }
            }
        }

        //Pulls data out of the Kethane database to update the SCANsat resource coverage map - needs some method to slow this down
        private void rebuildResourceArray ()
        {
            if (printer) {
                print("[Kethane Watcher] Rebuilding SCANsat Kethane Array");
                printer = false;
            }
            for (int ilon = 120 * rebuildFrame; ilon < 120 * (rebuildFrame + 1); ilon++) //Run sixty points per frame; 18 seconds at 60FPS
            {
                Cell cell = getKethaneCell(ilon, rebuildStep);
                if (KethaneData.Current.Scans[resource][body.name][cell])
                {
                    updateResourceArray(ilon, rebuildStep, SCANcontroller.controller.OverlayResourceType(resource));
                    ICellResource deposit = KethaneData.Current.GetCellDeposit(resource, body, cell);
                    if (deposit != null) updateResourceValue(ilon, rebuildStep, deposit.Quantity);
                    else updateResourceValue(ilon, rebuildStep, -1d); //Give empty cells -1 resources, fix later in the UI
                }
            }
            rebuildFrame++;
            if (rebuildFrame >= 3) {
                rebuildStep++;
                rebuildFrame = 0;
            }
            if (rebuildStep >= 180) {
                SCANcontroller.controller.kethaneBusy = false;
                printer = true;
                print("[Kethane Watcher] SCANsat Kethane Array Rebuilt");
            }
        }

        //Reset the resource value array - quicker than rebuildKethaneData (), called on map resets
        private void rebuildResourceValue (SCANdata.SCANResourceType type)
        {
            if (printera) {
                print("[Kethane Watcher] Rebuilding Value Array");
                printera = false;
            }
            SCANdata data = SCANcontroller.controller.getData (body);
            //for (int ilat = 0; ilat < 180; ilat++)
            //{
                for (int ilon = 0; ilon < 360; ilon++)
                {
                    if (data.isCoveredResource(ilon, rebuildValueStep, type))
                    {
                        //if (data.resourceCoverage[ilon, ilat] == 0) //Only check unassigned values
                        //{
                            Cell cell = getKethaneCell(ilon, rebuildValueStep);
                            ICellResource deposit = KethaneData.Current.GetCellDeposit(resource, body, cell); 
                            if (deposit != null) updateResourceValue (ilon, rebuildValueStep, deposit.Quantity);
                            else updateResourceValue (ilon, rebuildValueStep, -1d); //Give empty cells -1 resources, fix later in the UI
                        //}
                    }
                }
                rebuildValueStep++;
                if (rebuildValueStep >= 180) {
                    print("[Kethane Watcher] Value Array Rebuilt");
                    printera = true;
                }
            //}
        }

        private Cell getKethaneCell (int ilon, int ilat) //Find the Kethane cell corresponding to the current position
        {
            Vector3 pos = body.GetWorldSurfacePosition((double)ilat - 90, (double)ilon - 180, 50000d); //Set high altitude just to make sure the vector is above the surface
            Vector3 Wpos = body.transform.InverseTransformPoint(pos); //Draws a line from the position defined above through the center of the planet
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
            data.kethaneValueMap[lon, lat] = (float)value;
        }
    }
}
