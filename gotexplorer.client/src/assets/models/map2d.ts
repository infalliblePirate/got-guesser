import * as L from 'leaflet';

export class Map2d {
    private map: L.Map;
    private imageBounds: [[number, number], [number, number]];
    private imageUrl: string;
    private mapClickCallback!: (lat: number, lng: number) => void;

    constructor(imageUrl: string, imageBounds: [[number, number], [number, number]], containerId: string) {
        this.imageUrl = imageUrl;
        this.imageBounds = imageBounds;
        this.map = L.map(containerId, {
            crs: L.CRS.Simple,
            minZoom: -2,
            maxZoom: 2,
            center: this.calculateCenter(imageBounds),
            zoom: 2,
        });

        this.addImageOverlay();
        this.setupEventListeners();
    }

    private calculateCenter(bounds: [[number, number], [number, number]]): L.LatLng {
        const latCenter = (bounds[0][0] + bounds[1][0]) / 2;
        const lngCenter = (bounds[0][1] + bounds[1][1]) / 2;
        return new L.LatLng(latCenter, lngCenter);
    }

    private addImageOverlay(): void {
        L.imageOverlay(this.imageUrl, this.imageBounds).addTo(this.map);
        this.map.fitBounds(this.imageBounds);
    }

    private setupEventListeners(): void {
        this.map.on('click', this.handleMapClick.bind(this));
    }

    public handleMapClick(e: L.LeafletMouseEvent): void {
        const coords = e.latlng;
        if (
            coords.lng >= this.imageBounds[0][1] && coords.lng <= this.imageBounds[1][1] &&
            coords.lat >= this.imageBounds[0][0] && coords.lat <= this.imageBounds[1][0]
        ) {
            if(this.mapClickCallback) {
                this.mapClickCallback(coords.lat, coords.lng);
            }
        }
    }

    public onMapClick(callback: (lat: number, lng: number) => void): void {
        this.mapClickCallback = callback;
    }

    public showPopup(lat: number, lng: number, message: string): void {
        L.marker([lat, lng])
            .addTo(this.map)
            .bindPopup(message)
            .openPopup();
    }
}