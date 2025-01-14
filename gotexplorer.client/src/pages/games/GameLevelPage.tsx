import { useEffect } from "react";
import { Scene } from "../../assets/models/scene";
import { Map2d } from "../../assets/models/map2d";
import { GameLogic } from "../../assets/models/GameLogic";
import './GameLevelPage.scss';
import "../../../node_modules/leaflet/dist/leaflet.css";

const GameLevelPage = () => {
    useEffect(() => {
        const container = document.getElementById("three");
        if (!container) {
            console.error("Container for 3D scene not found.");
            return;
        }

        // scene
        const scene = new Scene(container);
        scene.loadModel("/assets/the-wall-with-lights.glb");
        scene.loadBackground("/assets/panorama.jpg");
        scene.animate();

        // map2d
        const imageBounds: [[number, number], [number, number]] = [[0, 0], [1080, 720]];

        const containerId = 'map';
        const map = new Map2d("/assets/map.png", imageBounds, containerId);
        
        const targetLocation = { lat: 964, lng: 380};
        const radius = 60;
        const gameLogic = new GameLogic(map, targetLocation, radius);
        gameLogic.startGame();

    }, []);

    return (
       <div className="model-and-map-container" id="container">
            <div id="three-container">
                 <div className="model-3d-container" id="three"></div>
             </div>
             <div id="map-container">
                 <div className="map-2d-container" id="map"></div>
             </div>
         </div>
    );
};

export default GameLevelPage;