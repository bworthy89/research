// Seeded random number generator for reproducible results
class SeededRandom {
    constructor(seed) {
        this.seed = seed;
    }

    next() {
        this.seed = (this.seed * 9301 + 49297) % 233280;
        return this.seed / 233280;
    }

    range(min, max) {
        return min + this.next() * (max - min);
    }
}

// Road types with colors and widths
const RoadTypes = {
    ARTERIAL: { color: '#ff4444', width: 8, name: 'Arterial' },
    COLLECTOR: { color: '#ffaa00', width: 5, name: 'Collector' },
    LOCAL: { color: '#ffffff', width: 3, name: 'Local' }
};

class RoadLayoutGenerator {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.roads = [];
        this.stats = {
            arterial: 0,
            collector: 0,
            local: 0
        };
    }

    clear() {
        this.ctx.fillStyle = 'rgba(0, 0, 0, 0)';
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this.roads = [];
        this.stats = { arterial: 0, collector: 0, local: 0 };
    }

    drawRoad(x1, y1, x2, y2, type) {
        const roadType = RoadTypes[type];
        this.ctx.strokeStyle = roadType.color;
        this.ctx.lineWidth = roadType.width;
        this.ctx.lineCap = 'round';
        this.ctx.beginPath();
        this.ctx.moveTo(x1, y1);
        this.ctx.lineTo(x2, y2);
        this.ctx.stroke();

        this.roads.push({ x1, y1, x2, y2, type });
        this.stats[type.toLowerCase()]++;
    }

    // Convert meters to canvas pixels
    metersToPixels(meters, mapSize) {
        return (meters / mapSize) * this.canvas.width;
    }

    generateGrid(params) {
        this.clear();

        const { size, blockWidth, blockHeight, arterialSpacing, randomization, seed } = params;
        const rng = new SeededRandom(seed);
        const canvasSize = this.canvas.width;

        // Helper function to add small randomization (in pixels, not percentage)
        const randomize = (value) => {
            if (randomization === 0) return value;
            const maxVariation = 20 * (randomization / 100); // Max 20 pixels at 100%
            return value + rng.range(-maxVariation, maxVariation);
        };

        // Draw vertical roads
        for (let x = 0; x <= size; x += blockWidth) {
            const isArterial = arterialSpacing > 0 && x % arterialSpacing === 0 && x !== 0;
            const roadType = isArterial ? 'ARTERIAL' : 'LOCAL';

            const xPos = this.metersToPixels(x, size);
            const xPosRandomized = randomize(xPos);
            const y1 = 0;
            const y2 = canvasSize;

            this.drawRoad(xPosRandomized, y1, xPosRandomized, y2, roadType);
        }

        // Draw horizontal roads
        for (let y = 0; y <= size; y += blockHeight) {
            const isArterial = arterialSpacing > 0 && y % arterialSpacing === 0 && y !== 0;
            const roadType = isArterial ? 'ARTERIAL' : 'LOCAL';

            const yPos = this.metersToPixels(y, size);
            const yPosRandomized = randomize(yPos);
            const x1 = 0;
            const x2 = canvasSize;

            this.drawRoad(x1, yPosRandomized, x2, yPosRandomized, roadType);
        }
    }

    generateAngledGrid(params) {
        this.clear();

        const { size, blockWidth, blockHeight, arterialSpacing, randomization, seed } = params;
        const rng = new SeededRandom(seed);
        const canvasSize = this.canvas.width;
        const angle = 45; // degrees

        // Helper function to add randomization
        const randomize = (value) => {
            if (randomization === 0) return value;
            const variation = (randomization / 100) * value;
            return value + rng.range(-variation, variation);
        };

        // Convert angle to radians
        const rad = (angle * Math.PI) / 180;
        const cos = Math.cos(rad);
        const sin = Math.sin(rad);

        // Draw diagonal roads (one direction)
        const spacing1 = blockWidth * 1.414; // Adjust for diagonal
        for (let offset = -size; offset <= size * 2; offset += spacing1) {
            const isArterial = arterialSpacing > 0 && Math.abs(offset % arterialSpacing) < spacing1;
            const roadType = isArterial ? 'ARTERIAL' : 'LOCAL';

            const offsetPixels = randomize(this.metersToPixels(offset, size));

            // Calculate line endpoints
            const x1 = offsetPixels;
            const y1 = 0;
            const x2 = offsetPixels + canvasSize;
            const y2 = canvasSize;

            this.drawRoad(x1, y1, x2, y2, roadType);
        }

        // Draw diagonal roads (perpendicular direction)
        for (let offset = -size; offset <= size * 2; offset += spacing1) {
            const isArterial = arterialSpacing > 0 && Math.abs(offset % arterialSpacing) < spacing1;
            const roadType = isArterial ? 'ARTERIAL' : 'LOCAL';

            const offsetPixels = randomize(this.metersToPixels(offset, size));

            // Calculate line endpoints
            const x1 = offsetPixels;
            const y1 = canvasSize;
            const x2 = offsetPixels + canvasSize;
            const y2 = 0;

            this.drawRoad(x1, y1, x2, y2, roadType);
        }
    }

    generateOrganic(params) {
        this.clear();

        const { size, blockWidth, arterialSpacing, randomization, seed } = params;
        const rng = new SeededRandom(seed);
        const canvasSize = this.canvas.width;

        // Generate main curved arterial roads
        if (arterialSpacing > 0) {
            const numArterials = Math.floor(size / arterialSpacing);

            for (let i = 1; i <= numArterials; i++) {
                const baseY = this.metersToPixels(i * arterialSpacing, size);
                const points = [];

                // Create curved path
                for (let x = 0; x <= canvasSize; x += canvasSize / 10) {
                    const variation = rng.range(-30, 30);
                    points.push({ x, y: baseY + variation });
                }

                // Draw smooth curve through points
                for (let j = 0; j < points.length - 1; j++) {
                    this.drawRoad(points[j].x, points[j].y, points[j + 1].x, points[j + 1].y, 'ARTERIAL');
                }
            }
        }

        // Generate organic local streets
        const numStreets = Math.floor(size / blockWidth);

        for (let i = 1; i < numStreets; i++) {
            const baseY = this.metersToPixels(i * blockWidth, size);
            const points = [];

            // Create more varied curved paths
            for (let x = 0; x <= canvasSize; x += canvasSize / 15) {
                const variation = rng.range(-50, 50);
                points.push({ x, y: baseY + variation });
            }

            // Draw smooth curve
            for (let j = 0; j < points.length - 1; j++) {
                this.drawRoad(points[j].x, points[j].y, points[j + 1].x, points[j + 1].y, 'LOCAL');
            }
        }

        // Add some connecting roads
        for (let i = 0; i < numStreets / 2; i++) {
            const x = rng.range(0, canvasSize);
            const y1 = rng.range(0, canvasSize / 3);
            const y2 = rng.range(canvasSize / 3, canvasSize);

            this.drawRoad(x, y1, x + rng.range(-50, 50), y2, 'LOCAL');
        }
    }

    generateRadial(params) {
        this.clear();

        const { size, arterialSpacing, randomization, seed } = params;
        const rng = new SeededRandom(seed);
        const canvasSize = this.canvas.width;
        const centerX = canvasSize / 2;
        const centerY = canvasSize / 2;

        // Number of radial roads
        const numRadials = 8;
        const angleStep = (2 * Math.PI) / numRadials;

        // Draw radial roads from center
        for (let i = 0; i < numRadials; i++) {
            const angle = i * angleStep + (randomization > 0 ? rng.range(-0.1, 0.1) : 0);
            const length = canvasSize / 2;

            const x2 = centerX + Math.cos(angle) * length;
            const y2 = centerY + Math.sin(angle) * length;

            this.drawRoad(centerX, centerY, x2, y2, 'ARTERIAL');
        }

        // Draw concentric circles
        const numRings = arterialSpacing > 0 ? Math.floor(size / arterialSpacing) : 4;
        const maxRadius = canvasSize / 2;

        for (let ring = 1; ring <= numRings; ring++) {
            const radius = (maxRadius / numRings) * ring;
            const roadType = ring % 2 === 0 ? 'COLLECTOR' : 'LOCAL';

            // Draw circle as series of line segments
            const segments = 64;
            for (let i = 0; i < segments; i++) {
                const angle1 = (i / segments) * 2 * Math.PI;
                const angle2 = ((i + 1) / segments) * 2 * Math.PI;

                const x1 = centerX + Math.cos(angle1) * radius;
                const y1 = centerY + Math.sin(angle1) * radius;
                const x2 = centerX + Math.cos(angle2) * radius;
                const y2 = centerY + Math.sin(angle2) * radius;

                this.drawRoad(x1, y1, x2, y2, roadType);
            }
        }
    }

    exportPNG(filename) {
        const link = document.createElement('a');
        link.download = filename || 'road-layout.png';
        link.href = this.canvas.toDataURL('image/png');
        link.click();
    }

    getStats() {
        return this.stats;
    }
}

// Initialize app
const generator = new RoadLayoutGenerator('roadCanvas');

// UI Elements
const patternSelect = document.getElementById('pattern');
const sizeInput = document.getElementById('size');
const blockWidthInput = document.getElementById('blockWidth');
const blockHeightInput = document.getElementById('blockHeight');
const arterialSpacingInput = document.getElementById('arterialSpacing');
const randomizationInput = document.getElementById('randomization');
const randomizationValue = document.getElementById('randomizationValue');
const seedInput = document.getElementById('seed');
const randomSeedBtn = document.getElementById('randomSeed');
const generateBtn = document.getElementById('generate');
const exportBtn = document.getElementById('export');
const statsDisplay = document.getElementById('stats');

// Update randomization display
randomizationInput.addEventListener('input', (e) => {
    randomizationValue.textContent = e.target.value + '%';
});

// Random seed button
randomSeedBtn.addEventListener('click', () => {
    seedInput.value = Math.floor(Math.random() * 1000000);
});

// Generate layout
generateBtn.addEventListener('click', () => {
    const params = {
        size: parseInt(sizeInput.value),
        blockWidth: parseInt(blockWidthInput.value),
        blockHeight: parseInt(blockHeightInput.value),
        arterialSpacing: parseInt(arterialSpacingInput.value),
        randomization: parseInt(randomizationInput.value),
        seed: parseInt(seedInput.value)
    };

    console.log('Generating with params:', params);

    const pattern = patternSelect.value;

    switch (pattern) {
        case 'grid':
            generator.generateGrid(params);
            break;
        case 'grid-angled':
            generator.generateAngledGrid(params);
            break;
        case 'organic':
            generator.generateOrganic(params);
            break;
        case 'radial':
            generator.generateRadial(params);
            break;
    }

    const stats = generator.getStats();
    statsDisplay.textContent = `Generated: ${stats.arterial} arterial, ${stats.collector} collector, ${stats.local} local roads`;
    console.log('Generation complete:', stats);
});

// Export PNG
exportBtn.addEventListener('click', () => {
    const pattern = patternSelect.value;
    const size = sizeInput.value;
    const seed = seedInput.value;
    const filename = `cs2-roads-${pattern}-${size}m-seed${seed}.png`;
    generator.exportPNG(filename);
});

// Generate initial layout
generateBtn.click();
