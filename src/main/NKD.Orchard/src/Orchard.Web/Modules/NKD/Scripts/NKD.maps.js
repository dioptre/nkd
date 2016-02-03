
// Construct the map object, and register events
function SetupMap(elm) {
    if (typeof google === 'undefined' || typeof google.maps === 'undefined' || typeof google.maps.LatLng === 'undefined') {
        console.log('failed to setup map');
        return false;
    }
    sydney = new google.maps.LatLng(-34.397, 150.644);
    geocoder = new google.maps.Geocoder();
    var map = new google.maps.Map(document.getElementById(elm), {
        zoom: 5,
        center: sydney,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    map.vname = elm;
    map.bounds = new google.maps.LatLngBounds();
    map.infowindow = new google.maps.InfoWindow();
    map.timeout = 50;
    map.CheckBoundsTimer = setInterval(function () { CheckBounds(map) }, map.timeout);
    map.boundsPoly = {};
    map.boundsCheckObj = {};
    map.CheckedBounds = 0;
    map.drawDebugViewportPoly = false;
    map.mapOverlays = new Array(); //ALL THE MAP DATA IS STORED HERE
    map.selectedShape = {};
    map.geocoder = {};
    map.sydney = {};
    map.path = new google.maps.MVCArray;
    map.pageIsLoaded = false;

    //google.maps.event.addListener(map, 'idle', function () {
    //}); //time in ms, that will reset if next 'bounds_changed' call is send, otherwise code will be executed after that time is up

    google.maps.event.addListener(map, 'dragstart', function () {
        map.CheckedBounds = 1;
    }); 

    //google.maps.event.addListener(map, 'dragend', function () {
    //});

    google.maps.event.addListener(map, 'zoom_changed', function () {
        map.CheckedBounds = 1;
    });

    //RedrawMap(map);


    return map;
}

function SetupDrawingMap(elm, drawingModes, map) {
    if (!map)
        map = SetupMap(elm);
    if (!map)
        return false;
    map.setZoom(8);
    map.drawingManager = {};
    if (!drawingModes)
        drawingModes = [google.maps.drawing.OverlayType.MARKER];
    //google.maps.drawing.OverlayType.POLYGON,
    //google.maps.drawing.OverlayType.POLYLINE

    //map.setMapTypeId(google.maps.MapTypeId.SATELLITE);

    //poly = new google.maps.Polygon({
    //    strokeWeight: 3,
    //    fillColor: '#5555FF'

    //});
    //poly.setMap(map);
    //poly.setPaths(new google.maps.MVCArray([path]));
    //poly.setOptions({
    //    editable: true
    //});

    //        google.maps.event.addListener(poly, 'mouseover', function () {
    //            poly.setOptions({
    //                editable: true
    //            });
    //        });
    //        google.maps.event.addListener(poly, 'mouseout', function () {
    //            poly.setOptions({
    //                editable: false
    //            });
    //        });

    //  google.maps.event.addListener(map, 'click', addPoint);

    //,        google.maps.drawing.OverlayType.RECTANGLE

    map.drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: google.maps.drawing.OverlayType.POLYGON,
        drawingControl: true,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: drawingModes
        },
        polygonOptions: {
            fillColor: '#02538a',
            tr: 0.5,
            fillOpacity: 0.5,
            strokeWeight: 1,
            clickable: true,
            zIndex: 1,
            editable: true
        },
        markerOptions: {
            fillColor: '#02538a',
            clickable: true,
            zIndex: 1,
            draggable: true
        }

    });


    google.maps.event.addListener(map.drawingManager, "overlaycomplete", function (e) {
        OverlayDone(map, e);
    });


    //        google.maps.event.addListener(drawingManager, 'overlaycomplete', function (e) {
    //            if (e.type != google.maps.drawing.OverlayType.MARKER) {
    //                // Switch back to non-drawing mode after drawing a shape.
    //                drawingManager.setDrawingMode(null);

    //                // Add an event listener that selects the newly-drawn shape when the user
    //                // mouses down on it.
    //                var newShape = e.overlay;
    //                newShape.type = e.type;
    //                google.maps.event.addListener(newShape, 'click', function () {
    //                    SetSelection(newShape);
    //                });
    //                SetSelection(newShape);
    //            }
    //        });
    google.maps.event.addListener(map.drawingManager, 'drawingmode_changed', function (e) {
        ClearSelection(map)
    });
    google.maps.event.addListener(map, 'click', function (e) {
        ClearSelection(map)
    });
    // google.maps.event.addDomListener(document.getElementById('delete-button'), 'click', DeleteSelectedShape);


    map.drawingManager.setMap(map);
    map.drawingManager.setDrawingMode(null);
    //google.maps.event.addListener(map, 'bounds_changed', function () {
    //    var bounds = map.getBounds();
    //    var ne = bounds.getNorthEast();
    //    var sw = bounds.getSouthWest();
    //    //do whatever you want with those bounds
    //    var textOut = ne.toString();
    //    document.getElementById("BoundsNE").value = textOut;
    //    textOut = sw.toString();
    //    document.getElementById("BoundsSW").value = textOut;

    //    //alert('Pan finished now bounds are ' + soutWest + 'S and ' + northEast);
    //});

    return map;

}

function HideDrawingMap(map) {
    if (typeof map !== 'undefined' && map.drawingManager) {
        map.drawingManager.setOptions({
            drawingControl: false
        });
    }
}

function ShowDrawingMap(map) {
    if (map.drawingManager) {
        map.drawingManager.setOptions({
            drawingControl: true
        });
    }
}



// Redraw a map.  This method removes all previous markers and looks a the google latlng opbjects in the 
// points array
function RedrawMap(map) {
    if (typeof google === 'undefined' || typeof google.maps === 'undefined' || typeof google.maps.LatLngBounds === 'undefined') {
        console.log('failed to redraw map');
        return false;
    }
    map.pageIsLoaded = false;
    map.bounds = new google.maps.LatLngBounds();
    // try and get existing bounds
    var boundsPassedIn = false;
    var bne = $('#BoundsNE').val();
    var bsw = $('#BoundsSW').val();
    
    if (bne != '') {
        boundsPassedIn = true;
    }
    DeleteShapes(map);
    var spatial = GetSpatialObjects();
    // if geography exist
    if (spatial.length > 0) {
        for (var i = 0; i < spatial.length; i++) {
            if (!spatial[i])
                continue;
            if (spatial[i].geography && HasPolygon(spatial[i].geography)) {
                var p = GetPolygonsFromGeography(spatial[i].geography);
                for (var j = 0; j < p.length; j++) {
                    AddPolygon(map, p[j], false, spatial[i].description, spatial[i].spatialid);
                    if (boundsPassedIn == false) {
                        for (var k =0; k < p[j].length; k++)
                            map.bounds.extend(p[j][k]);
                    }
                }
            }
            else if (spatial[i].centre) {
                AddMarker(map, spatial[i].centre, false, spatial[i].description, spatial[i].spatialid);
                // if no bounds defined (by a zoom or pan action) then manually expand the bounds to fit this marker
                if (boundsPassedIn == false) {
                    map.bounds.extend(spatial[i].centre);
                }
            }
        }

        if (boundsPassedIn == false) {
            map.fitBounds(bounds);
        } 

    }
    // draw a polygon that represents the current viewport
    if (map.drawDebugViewportPoly) {
        var mbounds = map.getbounds();
        if (mbounds != null) {
            var ne = mbounds.getnortheast();
            var sw = mbounds.getsouthwest();
            var boundscoords = createboundspolygon(ne, sw);

            if (boundspoly) {
                boundspoly.setmap(null);
            }
            // construct the polygon
            boundspoly = new google.maps.polygon({
                paths: boundscoords,
                strokecolor: '#0000ff',
                strokeopacity: 0.8,
                strokeweight: 2,
                fillcolor: '#ff0000',
                fillopacity: 0.2
            });
            boundspoly.setmap(map);
        }
    }
    map.pageIsLoaded = true;
}

function RefocusMap(map) {
    var markerCount = 0;
    var maxLat = -90;
    var maxLng = -180;
    var minLat = 90;
    var minLng = 180;
    var bounds = new google.maps.LatLngBounds();
    for (var k = 0; k < map.mapOverlays.length; k++) {
        if (map.mapOverlays[k] instanceof google.maps.Polygon) {
            var polygon = map.mapOverlays[k];
            for (var i = 0; i < polygon.getPaths().getLength() ; i++) {
                for (var j = 0; j < polygon.getPaths().getAt(i).getLength() ; j++) {
                    var lat = polygon.getPaths().getAt(i).getAt(j).lat();
                    var lng = polygon.getPaths().getAt(i).getAt(j).lng();
                    if (lat > maxLat)
                        maxLat = lat;
                    if (lat < minLat)
                        minLat = lat;
                    if (lng > maxLng)
                        maxLng = lng;
                    if (lng < minLng)
                        minLng = lng;
                    bounds.extend(polygon.getPaths().getAt(i).getAt(j));
                }
            }
        }
        else if (map.mapOverlays[k] instanceof google.maps.Marker) {
            var lat = map.mapOverlays[k].position.lat();
            var lng = map.mapOverlays[k].position.lng();
            if (lat > maxLat)
                maxLat = lat;
            if (lat < minLat)
                minLat = lat;
            if (lng > maxLng)
                maxLng = lng;
            if (lng < minLng)
                minLng = lng;
            bounds.extend(map.mapOverlays[k].position);
            markerCount++;
        }
    }    
    if (map.markerCount == 1 && map.mapOverlays.length == 1) {
        map.setCenter(new google.maps.LatLng(minLat + ((maxLat - minLat) / 2), minLng + ((maxLng - minLng) / 2)));
        map.setZoom(8);
    }
    else {
        //bounds.extend(new google.maps.LatLng(minLat, minLng));
        //bounds.extend(new google.maps.LatLng(maxLat, maxLng));
        map.fitBounds(bounds);
    }
}
// Check the bounds of the current map, and if they have changed force the location data table to updte
function CheckBounds(map) {
    if (map) {
        var localBounds = map.getBounds();
        if (!localBounds)
            return;
        if (!map.boundsCheckObj) {
            map.boundsCheckObj = localBounds;
            return;
        }
        if (map.boundsCheckObj.toString() != localBounds.toString()) {
            //get ready to redraw
            map.CheckedBounds = 1;
        }
        if (map.CheckedBounds > 0 && map.boundsCheckObj.toString() == localBounds.toString()) {
            if (map.CheckedBounds % 6 == 0) { //300msec = 6*50msec from poll above 'CheckBoundsTimer'
                //alert('Bounds have changed - do an update');
                map.CheckedBounds = 0;
                MapUpdated(map,{ eventType: 'BOUNDS_CHANGED' });
                return;
            }
            else {
                map.CheckedBounds++;
            }
        }
        map.boundsCheckObj = localBounds;
    }
}

// this is called when the lcoation data source has been updated and the map needs refreshing
// The locations are found in the HTML, and parsed into google maps compatible objects
function UpdateMap(map) {
    RedrawMap(map);
}

// Create a polygon that represents the map bounds - useful for debugging dynamic searching etc.
function CreateBoundsPolygon(ne, sw) {
    var x1 = ne.lng();
    var y1 = ne.lat();
    var x2 = sw.lng();
    var y2 = sw.lat();

    var boundCoords = [
          new google.maps.LatLng(y1, x1),
          new google.maps.LatLng(y2, x1),
          new google.maps.LatLng(y2, x2),
          new google.maps.LatLng(y1, x2),
          new google.maps.LatLng(y1, x1)
    ];

    //Bermuda Triangle Test
    var triangleCoords = [new google.maps.LatLng(25.774252, -80.190262), new google.maps.LatLng(18.466465, -66.118292), new google.maps.LatLng(32.321384, -64.75737), new google.maps.LatLng(25.774252, -80.190262)];

    return boundCoords;
}

// This is called when the map has moved or zoomed.  The relevent data (the map bounds and centre point) 
// are collated and stored in page variables, so that the data source can grab them during fresh web queries.
function MapUpdated(map, event) {
    if (!map)
        return;
    var bounds = map.getBounds();
    if (bounds) {
        var ne = bounds.getNorthEast();
        var sw = bounds.getSouthWest();
        var center = map.center;
        $('#BoundsNE').val(ne.toString());
        $('#BoundsSW').val(sw.toString());
        $('#CentreString').val(center.toString());
        var viewport = ne + "," + sw;
        OnMapUpdate(map, event, center.toString(), viewport);
    }
    else {
        OnMapUpdate(map, event);
    }
}

// Add a parker to the page.  Each marker will be assigned an event which will cause the popup to appear on click
function AddMarker(map, location, editable, popupText, id) {
    try {
        if (!editable)
            editable = false;
        var marker = new google.maps.Marker({
            position: location,
            map: map,
            draggable: editable,
            title: popupText,
            type: 'marker'
        });
        marker.uniqueid = id;
        map.mapOverlays.push(marker);
        SetSelection(map, marker);
        AddMarkerClickEvent(map, marker, popupText);
        if (editable)
            AddMarkerDragEvent(map, marker);
    } catch (err) {
        alert("Error with data " + err);
    }
}

function AddMarkerClickEvent(map, marker, popupText) {
    google.maps.event.addListener(marker, 'click', function (event) {
        if (popupText != null && popupText != "") {
            map.infowindow.setContent("<html><body><br><b>" + popupText + "</b></body></html>");
            map.infowindow.setPosition(event.latLng);
            map.infowindow.open(map, marker);
        }
        SetSelection(map, marker);
        MapUpdated(map, { eventType: 'MARKER_CLICKED', eventSource: marker });
    });
}

function AddMarkerDragEvent(map, marker) {
    google.maps.event.addListener(marker, 'dragend', function () {
        MapUpdated(map, { eventType: 'MARKER_DRAGEND', eventSource: marker });
    });
}

function AddMarkerSingle(map, location, editable, popupText, id) {
    AddMarker(map, location, editable, popupText, id);
    StopEditingMap(map);
}

function AddMarkerUnique(map, location, editable, popupText, id) {
    DeleteShapes(map);
    AddMarkerSingle(map, location, editable, popupText, id);
}

function AddPolygon(map, polygonArray, editable, popupText, id, colour) {
    //grep should be replaced with tha call to your backend for getting data for fk_id
    if (!colour)
        colour = "#FF8800";
    if (!editable)
        editable = false;
    var polygon = new google.maps.Polygon({
        paths: polygonArray,
        strokeColor: colour,
        strokeOpacity: 0.8,
        strokeWeight: 3,
        fillColor: colour,
        fillOpacity: 0.35,
        editable: editable,
        type: 'polygon'
    });
    polygon.uniqueid = id;
    polygon.setMap(map);
    map.mapOverlays.push(polygon);
    SetSelection(map, polygon);
    AddPolygonClickEvent(map, polygon, popupText);
    if (editable) {
        AddPolygonDragEvent(map, polygon);
        AddPolygonVertexEvent(map, polygon);
    }
}

function AddPolygonClickEvent(map, polygon, popupText) {
    google.maps.event.addListener(polygon, 'click', function () {
        SetSelection(map, polygon);
        if (popupText != null && popupText != "") {
            map.infowindow.setMap(null);
            //infowindow = new google.maps.InfoWindow();
            map.infowindow.setContent("<html><body><br><b>" + popupText + "</b></body></html>");
            map.infowindow.setPosition(FindCentre(polygon));
            map.infowindow.open(map);
        }
        MapUpdated(map, { eventType: 'POLYGON_CLICKED', eventSource: polygon });
    });
}

function AddPolygonDragEvent(map, polygon) {
    google.maps.event.addListener(polygon, 'dragend', function () {
        MapUpdated(map,{ eventType: 'POLYGON_DRAGEND', eventSource: polygon });
    });
}

function AddPolygonVertexEvent(map, polygon) {
    google.maps.event.addListener(polygon.getPath(), 'set_at', function (index) {
        MapUpdated(map,{ eventType: 'POLYGON_VERTEX_SET', eventSource: polygon });
    });
    google.maps.event.addListener(polygon.getPath(), 'remove_at', function (index) {
        MapUpdated(map,{ eventType: 'POLYGON_VERTEX_REMOVED', eventSource: polygon });
    });
    google.maps.event.addListener(polygon.getPath(), 'insert_at', function (index) {
        MapUpdated(map,{ eventType: 'POLYGON_VERTEX_INSERTED', eventSource: polygon });
    });
}

function FindCentre(polygon) {
    var maxLat = -90;
    var maxLng = -180;
    var minLat = 90;
    var minLng = 180;
    for (var i = 0; i < polygon.getPaths().getLength() ; i++) {
        for (var j = 0; j < polygon.getPaths().getAt(i).getLength() ; j++) {
            var lat = polygon.getPaths().getAt(i).getAt(j).lat();
            var lng = polygon.getPaths().getAt(i).getAt(j).lng();
            if (lat > maxLat)
                maxLat = lat;
            if (lat < minLat)
                minLat = lat;
            if (lng > maxLng)
                maxLng = lng;
            if (lng < minLng)
                minLng = lng;

        }
    }
    return new google.maps.LatLng(minLat + ((maxLat - minLat) / 2), minLng + ((maxLng - minLng) / 2));
}


if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (str) {
        return !this.indexOf(str);
    }
}

function AddGeographyUnique(map, locationInput, editable, popupText, autoExtend, id) {
    DeleteShapes(map);
    var bounds = new google.maps.LatLngBounds();
    var geoData = ParseGeographyData(locationInput);
    if (HasPolygon(geoData)) {    
        var p = GetPolygonsFromGeography(geoData);
        for (var i = 0; i < p.length; i++) {
            AddPolygon(map, p[i], editable, popupText, id);
            if (autoExtend) {
                for (var j = 0; j < p[i].length; j++) {
                    bounds.extend(p[i][j]);
                }
            }
        }
        if (autoExtend)
            map.fitBounds(bounds);
    }
    else {
        AddMarkerSingle(map, GetFirstLocation(geoData), editable, popupText, id);
    }
}

// Get the coordinates from the page, that are stored within the .gvResults and .gvResultText divs
function GetSpatialObjects() {
    var spatial = [];
    //could use document.getElementsByClassName
    $('.gvResultSpatial').each(function (index, element) {
        if (!spatial[index]) spatial[index] = {};
        spatial[index].geography = ParseGeographyData(element.innerHTML);
    });
    $('.gvResultCentre').each(function (index, element) {
        if (!spatial[index]) spatial[index] = {};
        spatial[index].centre = GetLocation(element.innerHTML);
    });
    $('.gvResultText').each(function (index, element) {
        if (!spatial[index]) spatial[index] = {};
        spatial[index].description = element.innerHTML;
    });
    $('.gvResultName').each(function (index, element) {
        if (!spatial[index]) spatial[index] = {};
        spatial[index].name = element.innerHTML;
    });
    $('.gvResultSpatialID').each(function (index, element) {
        if (!spatial[index]) spatial[index] = {};
        spatial[index].spatialid = element.innerHTML;
    });
    //TODO: do locations and provinces and lines
    return spatial;
}

function ParseLatLong(lat, long) {
    var geoData = [];
    geoData.push({ geography: new google.maps.LatLng(lat, long), geographyType: 'point' });
    return geoData;
}


//LONGITUDE, LATITUDE as per SQL
// Using the text data, construct a valid google maps latlng recursive array 'GeoData'
function ParseGeographyData(locationInput) {
    var geoData = [];
    if (locationInput.match(/geometrycollection.?/i) != null) {
        var p = locationInput.toLowerCase().indexOf('geometrycollection');
        geoData.push(ParseGeographyData(locationInput.substring(p + 19)));
    }
    else if (locationInput.match(/polygon.?/i) != null) {
        var p = locationInput.toLowerCase().indexOf('polygon');
        var pe = locationInput.indexOf(')', p + 1);
        var pts = locationInput.substring(p + 8, pe).replace(/[\()]/g, '').split(',');
        if (pts.length > 0) {
            var polygon = [];
            for (var i = 0; i < pts.length; i++) {
                polygon.push(ParseGeographyData(pts[i].replace(/^\s+|\s+$/g, '')));
            }
            geoData.push({ geography: polygon, geographyType: 'polygon' });
        }
        geoData.push(ParseGeographyData(locationInput.substring(pe)));
    }
    // single points look like this: SRID=4326;POINT (-90.1704 42.95081)
    else if (locationInput.match(/point.?/i) != null) {
        var p = locationInput.toLowerCase().indexOf('point');
        var pe = locationInput.indexOf(')', p + 1);
        var pt = locationInput.substring(p + 6, pe).replace(/[\()]/g, '').replace(/[,;: +]/g, ':').split(':');
        geoData.push({ geography: new google.maps.LatLng(pt[1], pt[0]), geographyType: 'point' });
        geoData.push(ParseGeographyData(locationInput.substring(pe)));
    }
    else if (locationInput.match(/\n.?/) != null) {
        var pts = locationInput.split('\n');
        for (var i = 0; i < pts.length; i++) {
            geoData.push(ParseGeographyData(pts[i]));
        }
    }
    else {
        var pt = locationInput.replace(/[\()]/g, '').replace(/[,;: +]/g, ':').split(':');
        if (pt.length != 2)
            return null;
        geoData.push({ geography: new google.maps.LatLng(pt[1], pt[0]), geographyType: 'point' });
    }
    return geoData;
}

function GetFirstLocation(geoData) {
    if (!geoData)
        return null;
    var rgd;
    for (var i = 0; i < geoData.length; i++) {
        if (!geoData[i])
            continue;
        if (geoData[i] == null)
            continue;
        if (geoData[i].geographyType == 'point') {
            rgd = geoData[i].geography;
            return rgd;
        }
        if (geoData[i] instanceof Array) {
            rgd = GetFirstLocation(geoData[i]);
            if (rgd)
                return rgd;
        }
        if (geoData[i].geography instanceof Array) {
            rgd = GetFirstLocation(geoData[i].geography);
            if (rgd)
                return rgd;
        }
    }
    return rgd;
}

function GetLocation(locationInput) {
   return GetFirstLocation(ParseGeographyData(locationInput));
}

function HasPolygon(geoData) {
    for (var i = 0; i < geoData.length; i++) {
        if (!geoData[i])
            continue;
        if (geoData[i] == null)
            continue;
        if (geoData[i].geographyType == 'polygon') {
            return true;
        }
        if (geoData[i] instanceof Array) {
            var rgd = HasPolygon(geoData[i]);
            if (rgd)
                return true;
        }
        if (geoData[i].geography instanceof Array) {
            rgd = HasPolygon(geoData[i].geography);
            if (rgd)
                return rgd;
        }
       
    }
    return false;
}

function HasPolygonString(locationInput) {
    return locationInput.match(/polygon.?/i) != null;
}

function GetPolygons(locationInput) {
    GetPolygonsFromGeography(ParseGeographyData(locationInput));
}

function GetPolygonsFromGeography(geoData) {
    var polygonArray = [];
    GetPolygonsWithArray(geoData, polygonArray);
    for (var i = 0; i < polygonArray.length; i++) {
        for (var j = 0; j < polygonArray[i].length; j++) {
            polygonArray[i][j] = polygonArray[i][j][0].geography;
        }
    }
    return polygonArray;
}

function GetPolygonsWithArray(geoData, polygonArray) {
    for (var i = 0; i < geoData.length; i++) {
        if (!geoData[i])
            continue;
        if (geoData[i] == null)
            continue;
        if (geoData[i] instanceof Array) {
            GetPolygonsWithArray(geoData[i], polygonArray);
        }
        if (geoData[i].geography instanceof Array) {
            GetPolygonsWithArray(geoData[i].geography, polygonArray);
        }
        if (geoData[i].geographyType == 'polygon') {
            if (geoData[i].geography.length < 3)
                return;
            if (geoData[i].geography[0][0].geography.lat() != geoData[i].geography[geoData[i].geography.length - 1][0].geography.lat()
                || geoData[i].geography[0][0].geography.lng() != geoData[i].geography[geoData[i].geography.length - 1][0].geography.lng())
                geoData[i].geography.push(geoData[i].geography[0]);
            polygonArray.push(geoData[i].geography);
        }
    }
}

function GetAddressLocation(address, callee) {
    if (typeof geocoder === 'undefined') {
        console.log('failed to geocode');
        return false;
    }
    geocoder.geocode({ 'address': address }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            callee(new google.maps.LatLng(results[0].geometry.location.lat(), results[0].geometry.location.lng()));
        } 
    });
}


function ClearSelection(map) {
    if (map.selectedShape && map.selectedShape instanceof google.maps.Polygon) {
        map.selectedShape.setEditable(false);
    }
    map.selectedShape = null;
    if (map.pageIsLoaded)
        MapUpdated(map,{ eventType: 'SELECT_CLEARED', eventSource: null });
}


function DeleteSelectedShape(map) {
    DeleteShape(map.selectedShape);
}

function DeleteShape(map,shape) {
    if (!shape)
        return;
    HideShape(map, shape);
    for (var i = map.mapOverlays.length - 1; i >= 0; i--) {
        if (map.mapOverlays[i] && map.mapOverlays[i].uniqueid == shape.uniqueid) {
            delete map.mapOverlays[i];
            map.mapOverlays.splice(i, 1);
            break;
        }
    }
    MapUpdated(map,{ eventType: 'DELETED_SHAPE', eventSource: shape });
}

function DeleteExceptedShape(map, shape) {
    for (var i = map.mapOverlays.length - 1; i >= 0; i--) {
        if (map.mapOverlays[i] && map.mapOverlays[i].uniqueid != map.selectedShape.uniqueid) {
            HideShape(map,map.mapOverlays[i]);
            delete map.mapOverlays[i];
            map.mapOverlays.splice(i, 1);
        }
    }
    MapUpdated(map,{ eventType: 'DELETED_EXCEPTED_SHAPE', eventSource: shape });
}

function DeleteSelectedExceptedShapeTypes(map) {
    DeleteExceptedShapeTypes(map,map.selectedShape);
}

function DeleteExceptedShapeTypes(map,shape) {
    for (var i = map.mapOverlays.length - 1; i >= 0; i--) {
        if (map.mapOverlays[i] && map.mapOverlays[i].type != map.selectedShape.type) {
            HideShape(map,map.mapOverlays[i]);
            delete map.mapOverlays[i];
            map.mapOverlays.splice(i, 1);
        }
    }
    MapUpdated(map,{ eventType: 'DELETED_SHAPE_TYPE', eventSource: shape });
}

function HideSelectedShape(map) {
    HideShape(map, map.selectedShape);
}

function HideShape(map, shape) {
    if (!shape)
        return;
    shape.setMap(null);
    if (shape instanceof google.maps.Polygon) {
        //Do something custom
    }
    else if (shape instanceof google.maps.Marker) {
        //Do something custom
    }
    if (map.pageIsLoaded)
        MapUpdated(map,{ eventType: 'HIDE_SHAPE', eventSource: shape });
}


function DeleteShapes(map) {
    if (!map || !map.mapOverlays)
        return;
    for (var i = 0 ; i < map.mapOverlays.length; i++) {        
        map.mapOverlays[i].setMap(null);
        delete map.mapOverlays[i];
    }
    map.mapOverlays = [];
    MapUpdated(map,{ eventType: 'DELETED_ALL_SHAPES', eventSource: null });
}

function SetSelection(map, shape) {
    if (!map.drawingManager || !map.pageIsLoaded)
        return;
    ClearSelection(map);
    //selectedShape.setStrokeWeight(1);
    map.selectedShape = shape;
    if (map.selectedShape) {
        if (map.selectedShape instanceof google.maps.Polygon) {
            map.selectedShape.setEditable(true);
        }
    }
    //shape.setStrokeWeight(2);
    //selectColor(shape.get('fillColor') || shape.get('strokeColor'));
    map.drawingManager.changed();
    if (map.pageIsLoaded)
        MapUpdated(map,{ eventType: 'SELECT_CHANGED', eventSource: shape });
}


//LONGITUDE LATITUDE
function DrawFromText(map, locationInput, editable, popupText, autoUpdate) {
    try {
        pageIsLoaded = false;
        var bounds = new google.maps.LatLngBounds();
        if (!autoUpdate)
            autoUpdate = false;
        DeleteShapes(map);
        var rows = locationInput.replace(/\r\n/g, '\n').replace(/\n\r/g, '\n').replace(/[\t,;:]/g, ' ').replace(/^\s+|\s+$/g, '').split('\n');
        if (rows.length < 1)
            return;
        if (rows.length == 1) {
            var columns = rows[0].split(' ');
            if (columns < 2) {
                return;
            }
            if (columns.length == 2) {
                var pt = new google.maps.LatLng(columns[1], columns[0])
                if (autoUpdate)
                    bounds.extend(pt);
                AddMarker(map, pt, editable, popupText);
                return;
            }
            if (columns.length > 2 && columns.length % 2 == 0) {
                //Is a long string (no /n) - can only make a single polygon
                var polygonArray = [];
                for (var i = 0; i < columns.length; i = i + 2) {
                    if (isNaN(columns[i]) || isNaN(columns[i + 1])) {
                        return;
                    }
                    var pt = new google.maps.LatLng(columns[i + 1], columns[i]);
                    polygonArray.push(pt);
                    if (autoUpdate)
                        bounds.extend(pt);
                }
                AddPolygon(map, polygonArray, editable, popupText);               
            }
        }
        else {
            rows.push(null);
            var currentShape = [];
            for (var i = 0; i < rows.length; i++) {
                var recordOK = true;
                var columns;
                if (!rows[i])
                    recordOK = false;
                else 
                    columns = rows[i].split(' ');
                if (!recordOK || !columns || columns.length != 2) {
                    if (currentShape.length == 1) {
                        AddMarker(map, currentShape[0], editable, popupText);
                    }
                    else if (currentShape.length > 1) {
                        AddPolygon(map, currentShape, editable, popupText); //TODO: this should also include line-strings etc. (1st should repeat last)
                    }
                    currentShape = [];
                    continue;
                }
                else if (isNaN(columns[0]) || isNaN(columns[1])) {
                    return;
                }
                else {
                    var pt = new google.maps.LatLng(columns[1], columns[0]);
                    currentShape.push(pt);
                    if (autoUpdate)
                        bounds.extend(pt);
                }
            }
        }
    }
    catch (e) { }
    finally {
        map.pageIsLoaded = true;
        if (autoUpdate) {
            map.fitBounds(bounds);
            map.setCenter(bounds.getCenter());
        }
    }
}


function OverlayDone(map, event) {

    if (!event.overlay.uniqueid)
        event.overlay.uniqueid = NewGUID();
    event.overlay.title = "";
    event.overlay.content = "";
    event.overlay.type = event.type;
    map.mapOverlays.push(event.overlay);
    var newShape = event.overlay;
    newShape.type = event.type;
    if (map.pageIsLoaded)
        SetSelection(map,newShape);
    if (newShape instanceof google.maps.Marker) {
        newShape.setDraggable(true);
        AddMarkerClickEvent(map, newShape, '');
        AddMarkerDragEvent(map, newShape);
    }
    else if (newShape instanceof google.maps.Polygon) {
        AddPolygonClickEvent(map, newShape, '');
        AddPolygonDragEvent(map, newShape);
        AddPolygonVertexEvent(map, newShape);
    }
    MapUpdated(map,{ eventType: 'EDITED', eventSource: newShape });
    //AttachClickListener(event.overlay);
    //openInfowindow(event.overlay, getShapeCenter(event.overlay), getEditorContent(event.overlay));
}

function StopEditingMap(map) {
    if (map.drawingManager)
        map.drawingManager.setDrawingMode(null);
}


//LONGITUDE LATITUDE as per SQL Server
function MapObjectsToString(map) {
    var tmpMap = new Object;

    var outputString = "";
    var tmpOverlay, paths;
    tmpMap.zoom = map.getZoom();
    tmpMap.tilt = map.getTilt();
    tmpMap.mapTypeId = map.getMapTypeId();
    tmpMap.center = { lat: map.getCenter().lat(), lng: map.getCenter().lng() };
    tmpMap.overlays = new Array();

    for (var i = 0; i < map.mapOverlays.length; i++) {
        if (map.mapOverlays[i].getMap() != map) {
            continue;
        }
        tmpOverlay = new Object;
        tmpOverlay.type = map.mapOverlays[i].type;
        tmpOverlay.title = map.mapOverlays[i].title;
        tmpOverlay.content = map.mapOverlays[i].content;

        if (i > 0)
            outputString += "\n";

        if (map.mapOverlays[i].fillColor) {
            tmpOverlay.fillColor = map.mapOverlays[i].fillColor;
        }

        if (map.mapOverlays[i].fillOpacity) {
            tmpOverlay.fillOpacity = map.mapOverlays[i].fillOpacity;
        }

        if (map.mapOverlays[i].strokeColor) {
            tmpOverlay.strokeColor = map.mapOverlays[i].strokeColor;
        }

        if (map.mapOverlays[i].strokeOpacity) {
            tmpOverlay.strokeOpacity = map.mapOverlays[i].strokeOpacity;
        }

        if (map.mapOverlays[i].strokeWeight) {
            tmpOverlay.strokeWeight = map.mapOverlays[i].strokeWeight;
        }

        if (map.mapOverlays[i].icon) {
            tmpOverlay.icon = map.mapOverlays[i].icon;
        }

        if (map.mapOverlays[i].flat) {
            tmpOverlay.flat = map.mapOverlays[i].flat;
        }

        if (map.mapOverlays[i].type == "polygon" || typeof (map.mapOverlays[i]) == google.maps.Polygon) {
            tmpOverlay.paths = new Array();
            paths = map.mapOverlays[i].getPaths();
            for (var j = 0; j < paths.length; j++) {
                tmpOverlay.paths[j] = new Array();
                for (var k = 0; k < paths.getAt(j).length; k++) {
                    tmpOverlay.paths[j][k] = { lat: paths.getAt(j).getAt(k).lat().toString(), lng: paths.getAt(j).getAt(k).lng().toString() };
                    outputString += paths.getAt(j).getAt(k).lng().toString() + " " + paths.getAt(j).getAt(k).lat().toString() + "\n";
                }
            }

        } else if (map.mapOverlays[i].type == "polyline") {
            tmpOverlay.path = new Array();
            path = map.mapOverlays[i].getPath();
            for (var j = 0; j < path.length; j++) {
                tmpOverlay.path[j] = { lat: path.getAt(j).lat().toString(), lng: path.getAt(j).lng().toString() };
            }

        } else if (map.mapOverlays[i].type == "circle") {
            tmpOverlay.center = { lat: map.mapOverlays[i].getCenter().lat(), lng: map.mapOverlays[i].getCenter().lng() };
            tmpOverlay.radius = map.mapOverlays[i].radius;
        } else if (map.mapOverlays[i].type == "rectangle") {
            tmpOverlay.bounds = {
                sw: { lat: map.mapOverlays[i].getBounds().getSouthWest().lat(), lng: map.mapOverlays[i].getBounds().getSouthWest().lng() },
                ne: { lat: map.mapOverlays[i].getBounds().getNorthEast().lat(), lng: map.mapOverlays[i].getBounds().getNorthEast().lng() }
            };
        } else if (map.mapOverlays[i].type == "marker") {
            tmpOverlay.position = { lat: map.mapOverlays[i].getPosition().lat(), lng: map.mapOverlays[i].getPosition().lng() };
            outputString += map.mapOverlays[i].getPosition().lng() + " " + map.mapOverlays[i].getPosition().lat() + "\n";
        } else {
            alert("Unknown Map Type: " + map.mapOverlays[i].type);
        }
        tmpMap.overlays.push(tmpOverlay);
    }

    return outputString;

}

if (typeof deferredMap != 'undefined')
    deferredMap.resolve("Map Loaded");

// When the window is loaded, casue the map to intiialise, and populate it with table data
//window.onload = function () {
//    SetupMap();
//    UpdateMap();
//};