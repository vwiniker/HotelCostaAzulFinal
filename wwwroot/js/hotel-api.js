// Hotel Costa Azul - API Client
class HotelAPI {
    constructor() {
        this.baseURL = window.location.origin;
    }

    // Método genérico para hacer peticiones
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        };

        try {
            const response = await fetch(url, config);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    // === HABITACIONES ===
    async getHabitaciones() {
        return await this.request('/api/habitaciones');
    }

    async getHabitacionesDisponibles() {
        return await this.request('/api/habitaciones/disponibles');
    }

    async getHabitacionesPorTipo(tipo) {
        return await this.request(`/api/habitaciones/tipo/${tipo}`);
    }

    async getHabitacion(id) {
        return await this.request(`/api/habitaciones/${id}`);
    }

    // === CLIENTES ===
    async getClientes() {
        return await this.request('/api/clientes');
    }

    async crearCliente(cliente) {
        return await this.request('/api/clientes', {
            method: 'POST',
            body: JSON.stringify(cliente)
        });
    }

    async buscarClientePorEmail(email) {
        return await this.request(`/api/clientes/buscar/${email}`);
    }

    // === RESERVAS ===
    async getReservas() {
        return await this.request('/api/reservas');
    }

    async crearReserva(reserva) {
        return await this.request('/api/reservas', {
            method: 'POST',
            body: JSON.stringify(reserva)
        });
    }

    async getReserva(id) {
        return await this.request(`/api/reservas/${id}`);
    }

    async consultarDisponibilidad(fechaEntrada, fechaSalida) {
        const params = new URLSearchParams({
            fechaEntrada: fechaEntrada,
            fechaSalida: fechaSalida
        });
        return await this.request(`/api/reservas/disponibilidad?${params}`);
    }

    // === UTILIDADES ===
    formatearFecha(fecha) {
        return new Date(fecha).toISOString().split('T')[0];
    }

    formatearPrecio(precio) {
        return new Intl.NumberFormat('es-CR', {
            style: 'currency',
            currency: 'CRC'
        }).format(precio);
    }

    calcularNoches(fechaEntrada, fechaSalida) {
        const entrada = new Date(fechaEntrada);
        const salida = new Date(fechaSalida);
        const diferencia = salida.getTime() - entrada.getTime();
        return Math.ceil(diferencia / (1000 * 3600 * 24));
    }
}

// Instancia global de la API
const hotelAPI = new HotelAPI();

// === FUNCIONES DE UI ===

// Mostrar notificaciones
function mostrarNotificacion(mensaje, tipo = 'info') {
    // Crear el contenedor de notificaciones si no existe
    let container = document.getElementById('notificaciones-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'notificaciones-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
            max-width: 400px;
        `;
        document.body.appendChild(container);
    }

    const notificacion = document.createElement('div');
    notificacion.style.cssText = `
        background: ${tipo === 'success' ? '#48bb78' : tipo === 'error' ? '#f56565' : '#4299e1'};
        color: white;
        padding: 1rem;
        border-radius: 8px;
        margin-bottom: 10px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        transform: translateX(100%);
        transition: all 0.3s ease;
    `;

    notificacion.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center;">
            <span>${mensaje}</span>
            <button onclick="this.parentElement.parentElement.remove()" style="background: none; border: none; color: white; font-size: 1.2rem; cursor: pointer;">&times;</button>
        </div>
    `;

    container.appendChild(notificacion);

    // Animación de entrada
    setTimeout(() => {
        notificacion.style.transform = 'translateX(0)';
    }, 100);

    // Auto-remover después de 5 segundos
    setTimeout(() => {
        if (notificacion.parentElement) {
            notificacion.style.transform = 'translateX(100%)';
            setTimeout(() => notificacion.remove(), 300);
        }
    }, 5000);
}

// Mostrar loading
function mostrarLoading(elemento) {
    const loading = document.createElement('div');
    loading.className = 'loading-overlay';
    loading.innerHTML = `
        <div style="
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: rgba(255,255,255,0.9);
            padding: 2rem;
            border-radius: 10px;
            text-align: center;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        ">
            <div style="
                width: 40px;
                height: 40px;
                border: 4px solid #f3f3f3;
                border-top: 4px solid #3498db;
                border-radius: 50%;
                animation: spin 1s linear infinite;
                margin: 0 auto 1rem;
            "></div>
            <p>Cargando...</p>
        </div>
        <style>
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
        </style>
    `;
    loading.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0,0,0,0.1);
        z-index: 1000;
    `;

    elemento.style.position = 'relative';
    elemento.appendChild(loading);

    return loading;
}

function ocultarLoading(loading) {
    if (loading && loading.parentElement) {
        loading.remove();
    }
}

// Validar email
function validarEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

// Validar teléfono (formato Costa Rica)
function validarTelefono(telefono) {
    const regex = /^[\+]?[0-9\s\-\(\)]{8,15}$/;
    return regex.test(telefono);
}

console.log('🏨 Hotel Costa Azul API Client cargado correctamente');