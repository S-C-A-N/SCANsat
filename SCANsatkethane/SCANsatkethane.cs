/*
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsatKethane - Module for interfacing with Kethane assembly
 *
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */

using System;
using SCANsat;
using Kethane;
using UnityEngine;

namespace SCANsatKethane
{

    public class SCANsatKethane: MonoBehaviour
    {
        private CelestialBody body;
        private string resource;
        private bool rebuild, reset;
        private int rebuildStep, rebuildValueStep, rebuildFrame;
        private bool initialized, rebuildingArray, rebuildingValue = false;
                
        public void Start() {
            print("[SCAN Kethane] Starting Up");
            GameEvents.onFlightReady.Add(initialize);
        }

        public void OnDestroy() {
            print("[SCAN Kethane] Shutting Down");
            GameEvents.onFlightReady.Remove(initialize);
        }

        public void Update() {
            if (initialized) {
                if (rebuildStep < 180) {
                    rebuildResourceArray();
                }
                if (rebuildValueStep < 45) {
                    rebuildResourceValue(SCANcontroller.controller.OverlayResourceType(resource));
                }
                if (SCANcontroller.controller.resourceOverlayType == 1 && SCANcontroller.controller.ResourcesList.Count > 0 && !SCANcontroller.controller.kethaneBusy) {
                    setBody (FlightGlobals.currentMainBody);
                    setResource (SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection]);
                    setRebuild (SCANcontroller.controller.kethaneRebuild);
                    setReset (SCANcontroller.controller.kethaneReset);
                }
            }
        }

        private void initialize() {
            rebuild = SCANcontroller.controller.kethaneRebuild;
            print("[SCAN Kethane] Initialized");
            rebuildStep = 180;
            rebuildValueStep = 180;
            rebuildFrame = 0;
            initialized = true;
        }

        private void setBody(CelestialBody b) { //Watcher to check for Celestial Body changes
            if (body == b) return;
            if (b != null) {
            body = b;
            resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
            reset = SCANcontroller.controller.kethaneReset;
            SCANcontroller.controller.map_ResourceOverlay = false; //Force the overlay off to allow array to rebuild
            SCANdata data = SCANcontroller.controller.getData(body);
            data.kethaneValueMap = new float[360, 180];
            rebuildStep = 0;
            rebuildFrame = 0;
            SCANcontroller.controller.kethaneBusy = true;
            }
        }

        private void setResource(string s) { //Watcher to check for the user switching to different Kethane resources
            if (s == resource) return;
            resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
            reset = SCANcontroller.controller.kethaneReset;
            SCANcontroller.controller.map_ResourceOverlay = false; //Force the overlay off to allow array to rebuild
            SCANdata data = SCANcontroller.controller.getData (body);
            data.kethaneValueMap = new float[360, 180];
            rebuildStep = 0;
            rebuildFrame = 0;
            SCANcontroller.controller.kethaneBusy = true;
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
        private void updateKethaneData (SCANdata.SCANtype type) {
            print("[SCAN Kethane] Updating Kethane Database");
            SCANdata data = SCANcontroller.controller.getData (body);
            for (int ilat = 0; ilat < 180; ilat++) {
                for (int ilon = 0; ilon < 360; ilon++) {
                    if (data.isCovered (ilon - 180, ilat - 90, type)) {
                        Cell cell = getKethaneCell (ilon - 180, ilat - 90);
                        if (!KethaneData.Current.Scans[resource][body.name][cell])
                            KethaneData.Current.Scans[resource][body.name][cell] = true;
                    }
                }
            }
        }

        //Pulls data out of the Kethane database to update the SCANsat resource coverage map - intentionally slow
        private void rebuildResourceArray () {
            SCANdata data = SCANcontroller.controller.getData (body);
            if (!rebuildingArray) {
                print("[SCAN Kethane] Rebuilding SCANsat Kethane Array");
                rebuildingArray = true;
            }
            for (int ilon = 120 * rebuildFrame; ilon < 120 * (rebuildFrame + 1); ilon++) {//Run 120 points per frame; 9 seconds at 60FPS
                Cell cell = getKethaneCell(ilon - 180, rebuildStep - 90);
                if (KethaneData.Current.Scans[resource][body.name][cell]) {
                    updateResourceArray(ilon, rebuildStep, SCANcontroller.controller.OverlayResourceType(resource), data);
                    ICellResource deposit = KethaneData.Current.GetCellDeposit(resource, body, cell);
                    if (deposit != null) updateResourceValue(ilon, rebuildStep, deposit.Quantity, data);
                    else updateResourceValue(ilon, rebuildStep, -1d, data); //Give empty cells -1 resources, account for this later on
                }
            }
            rebuildFrame++;
            if (rebuildFrame >= 3) {
                rebuildStep++;
                rebuildFrame = 0;
            }
            if (rebuildStep >= 180) {
                SCANcontroller.controller.kethaneBusy = false;
                rebuildingArray = false;
                print("[SCAN Kethane] SCANsat Kethane Array Rebuilt");
            }
        }

        //Reset the resource value array - quicker than rebuildKethaneData (), called on map resets
        private void rebuildResourceValue (SCANdata.SCANtype type) {
            if (!rebuildingValue) {
                print("[SCAN Kethane] Rebuilding Value Array");
                rebuildingValue = true;
            }
            SCANdata data = SCANcontroller.controller.getData (body);
            for (int ilat = 4 * rebuildValueStep; ilat < 4 * (rebuildValueStep + 1); ilat++) {
                for (int ilon = 0; ilon < 360; ilon++) {
                    if (data.isCovered(ilon, ilat, type)) {
                        if (data.kethaneValueMap[ilon, ilat] == 0) { //Only check unassigned values
                            Cell cell = getKethaneCell(ilon - 180, ilat - 90);
                            ICellResource deposit = KethaneData.Current.GetCellDeposit(resource, body, cell); 
                            if (deposit != null) updateResourceValue (ilon, ilat, deposit.Quantity, data);
                            else updateResourceValue (ilon, ilat, -1d, data); //Give empty cells -1 resources, account for this later on
                        }
                    }
                }                
            }
            rebuildValueStep++;
            if (rebuildValueStep >= 45) {
                print("[SCAN Kethane] Value Array Rebuilt");
                rebuildingValue = false;
            }
        }

        private Cell getKethaneCell (int ilon, int ilat) {  //Find the Kethane cell corresponding to the current position 
            Vector3 pos = body.GetWorldSurfacePosition((double)ilat, (double)ilon, 50000d); //Set high altitude just to make sure the vector is above the surface
            Vector3 Wpos = body.transform.InverseTransformPoint(pos); //Draws a line from the position defined above through the center of the planet
            return Kethane.Cell.Containing(Wpos, 5);
        }

        private void updateResourceArray (int lon, int lat, SCANdata.SCANtype type, SCANdata data) {
            data.coverage[lon, lat] |= (Int32)type;
        }

        private void updateResourceValue (int lon, int lat, double value, SCANdata data) {
            data.kethaneValueMap[lon, lat] = (float)value;
        }
    }
}
