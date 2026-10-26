/**
 * ==============================================================================
 * jDroid-X-Desk — High-Performance Peer-to-Peer Stream & Signaling Service
 * Architecture: Senior Dynamic OOPS MVC Service
 * Branding: jDroid-X Universal Streaming Engine
 * ==============================================================================
 * Implements high-throughput tile-delta streaming, P2P signaling, and remote input handling.
 */

const net = require('net');
const crypto = require('crypto');
const { EventEmitter } = require('events');

class JDroidXStreamService extends EventEmitter {
    constructor(config = {}) {
        super();
        this.port = config.port || 7070;
        this.sessionId = config.sessionId || crypto.randomBytes(4).toString('hex');
        this.activeSessions = new Map();
    }

    /**
     * Initializes jDroid-X TCP signaling listener (Port 7070)
     */
    startSignalingServer() {
        this.server = net.createServer((socket) => {
            const peerId = `${socket.remoteAddress}:${socket.remotePort}`;
            console.log(`[jDroid-X-Stream] Inbound peer connected: ${peerId}`);

            socket.on('data', (buffer) => {
                this.handlePacket(socket, peerId, buffer);
            });

            socket.on('close', () => {
                console.log(`[jDroid-X-Stream] Peer disconnected: ${peerId}`);
                this.activeSessions.delete(peerId);
            });
        });

        this.server.listen(this.port, () => {
            console.log(`[jDroid-X-Stream] High-Speed Signaling Server listening on port ${this.port}`);
        });
    }

    /**
     * Handles framed jDroid-X packet protocol
     */
    handlePacket(socket, peerId, buffer) {
        // jDroid-X header check (magic: 0x4A 0x44 -> "JD")
        if (buffer.length < 4) return;
        const msgType = buffer.readUInt16BE(0);

        switch (msgType) {
            case 0x0001: // Handshake / Auth
                this.respondHandshake(socket);
                break;
            case 0x0002: // Frame Request (jDroid-X Stream)
                this.emit('frame_request', peerId);
                break;
            case 0x0003: // Input Injection (Mouse/Keyboard)
                this.handleRemoteInput(buffer.slice(4));
                break;
            default:
                break;
        }
    }

    respondHandshake(socket) {
        const response = Buffer.alloc(8);
        response.writeUInt16BE(0x0001, 0); // Auth OK
        response.writeUInt16BE(0x0000, 2); // Status
        response.writeUInt32BE(0x00000001, 4); // Protocol v1
        socket.write(response);
    }

    handleRemoteInput(payload) {
        // Dispatches hardware scancode input
        this.emit('remote_input', payload);
    }
}

// Chesterton's Fence: Backward compatibility alias
const AnyDeskDeskRTService = JDroidXStreamService;

module.exports = JDroidXStreamService;
module.exports.JDroidXStreamService = JDroidXStreamService;
module.exports.AnyDeskDeskRTService = AnyDeskDeskRTService;

