import { Map2d } from './map2d';

export class GameLogic {
    private map: Map2d;
    private targetLocation: { lat: number; lng: number };
    private radius: number;
    private score: number;

    constructor(map: Map2d, targetLocation: { lat: number; lng: number }, radius: number) {
        this.map = map;
        this.targetLocation = targetLocation;
        this.radius = radius;
        this.score = 0;
    }
    public startGame(): void {
        this.map.onMapClick(this.handleMapClick.bind(this));
    }

    private handleMapClick(lat: number, lng: number): void {
        const distance = Math.sqrt(
            Math.pow(lat - this.targetLocation.lat, 2) +
            Math.pow(lng - this.targetLocation.lng, 2)
        );
        const isCorrect = distance < this.radius; 

        if (isCorrect) {
            this.score += 10; 
            this.map.showPopup(lat, lng, `${lat}, ${lng}, Correct! +10 points. Current score: ${this.score}`);
        } else {
            this.map.showPopup(lat, lng, `${lat}, ${lng}, Incorrect! No points. Current score: ${this.score}`);
        }
    }

    public getScore(): number {
        return this.score;
    }
}