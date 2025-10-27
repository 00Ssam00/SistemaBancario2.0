// ========================================
// ANIMACIONES Y EFECTOS
// ========================================

// Agregar clase fade-in a elementos cuando entran en viewport
document.addEventListener('DOMContentLoaded', function () {
    // Observador de intersección para animaciones
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
            }
        });
    }, observerOptions);

    // Observar todos los cards y elementos animables
    document.querySelectorAll('.card, .hero-section').forEach(el => {
        observer.observe(el);
    });

    // Auto-cerrar alertas después de 5 segundos
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Validar formato de moneda en inputs
    const moneyInputs = document.querySelectorAll('input[type="number"]');
    moneyInputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value) {
                // Redondear a miles
                this.value = Math.round(this.value / 1000) * 1000;
            }
        });
    });

    // Confirmación antes de operaciones importantes
    const transferBtn = document.querySelector('form[asp-page-handler="Transferir"] button[type="submit"]');
    if (transferBtn) {
        transferBtn.addEventListener('click', function (e) {
            const monto = document.querySelector('form[asp-page-handler="Transferir"] input[name="monto"]').value;
            const cuenta = document.querySelector('form[asp-page-handler="Transferir"] input[name="cuentaDestino"]').value;

            if (monto && cuenta) {
                const confirmar = confirm(`¿Estás seguro de transferir $${parseInt(monto).toLocaleString('es-CO')} a la cuenta ${cuenta}?`);
                if (!confirmar) {
                    e.preventDefault();
                }
            }
        });
    }

    // Formato de moneda en tiempo real
    const inputsMonto = document.querySelectorAll('input[name="monto"]');
    inputsMonto.forEach(input => {
        input.addEventListener('input', function () {
            // Mostrar preview del monto formateado
            const valor = parseInt(this.value) || 0;
            const formateado = valor.toLocaleString('es-CO');

            // Buscar o crear elemento de preview
            let preview = this.parentElement.querySelector('.money-preview');
            if (!preview) {
                preview = document.createElement('small');
                preview.className = 'money-preview text-muted d-block mt-1';
                this.parentElement.appendChild(preview);
            }

            if (valor > 0) {
                preview.textContent = `${formateado} COP`;
            } else {
                preview.textContent = '';
            }
        });
    });

    // Efecto de ripple en botones
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');

            this.appendChild(ripple);

            setTimeout(() => ripple.remove(), 600);
        });
    });

    // Tooltip para mostrar información adicional
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// ========================================
// FUNCIONES UTILITARIAS
// ========================================

// Formatear número a pesos colombianos
function formatMoney(value) {
    return new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0
    }).format(value);
}

// Copiar texto al portapapeles
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
        showToast('Copiado al portapapeles', 'success');
    }, function () {
        showToast('Error al copiar', 'danger');
    });
}

// Mostrar notificación toast
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const toastId = 'toast-' + Date.now();
    const toastHTML = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    document.getElementById('toastContainer').insertAdjacentHTML('beforeend', toastHTML);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Validar número de cuenta (5 dígitos)
function validateAccountNumber(input) {
    const value = input.value.replace(/\D/g, '');
    input.value = value.slice(0, 5);
    return value.length === 5;
}

// Validar clave (4 dígitos)
function validatePassword(input) {
    const value = input.value.replace(/\D/g, '');
    input.value = value.slice(0, 4);
    return value.length === 4;
}

// ========================================
// CALCULADORA DE CUOTAS (PARA CRÉDITO)
// ========================================

function calcularCuotas(monto, numeroCuotas) {
    let tasaInteres = 0;

    if (numeroCuotas <= 2) {
        tasaInteres = 0;
    } else if (numeroCuotas <= 6) {
        tasaInteres = 0.019; // 1.9%
    } else {
        tasaInteres = 0.023; // 2.3%
    }

    if (tasaInteres === 0) {
        return {
            cuotaMensual: monto / numeroCuotas,
            interesTotal: 0,
            totalPagar: monto
        };
    }

    // Fórmula de cuota fija con interés compuesto
    const factorInteres = Math.pow(1 + tasaInteres, numeroCuotas);
    const cuotaMensual = monto * (tasaInteres * factorInteres) / (factorInteres - 1);
    const totalPagar = cuotaMensual * numeroCuotas;
    const interesTotal = totalPagar - monto;

    return {
        cuotaMensual: cuotaMensual,
        interesTotal: interesTotal,
        totalPagar: totalPagar
    };
}

// Evento para calcular cuotas en el modal de compra con crédito
document.addEventListener('DOMContentLoaded', function () {
    const modalCompra = document.getElementById('modalCompraCredito');
    if (modalCompra) {
        const montoInput = modalCompra.querySelector('input[name="monto"]');
        const cuotasSelect = modalCompra.querySelector('select[name="cuotas"]');

        function actualizarCalculoCuotas() {
            const monto = parseFloat(montoInput.value) || 0;
            const cuotas = parseInt(cuotasSelect.value) || 1;

            if (monto > 0) {
                const calculo = calcularCuotas(monto, cuotas);

                // Buscar o crear elemento de resumen
                let resumen = modalCompra.querySelector('.resumen-cuotas');
                if (!resumen) {
                    resumen = document.createElement('div');
                    resumen.className = 'resumen-cuotas alert alert-info mt-3';
                    cuotasSelect.parentElement.insertAdjacentElement('afterend', resumen);
                }

                resumen.innerHTML = `
                    <h6 class="fw-bold mb-2"><i class="fas fa-calculator me-2"></i>Resumen de la Compra</h6>
                    <div class="row g-2">
                        <div class="col-6">
                            <small class="text-muted">Monto:</small><br>
                            <strong>${formatMoney(monto)}</strong>
                        </div>
                        <div class="col-6">
                            <small class="text-muted">Cuota mensual:</small><br>
                            <strong class="text-primary">${formatMoney(calculo.cuotaMensual)}</strong>
                        </div>
                        <div class="col-6">
                            <small class="text-muted">Interés total:</small><br>
                            <strong>${formatMoney(calculo.interesTotal)}</strong>
                        </div>
                        <div class="col-6">
                            <small class="text-muted">Total a pagar:</small><br>
                            <strong class="text-danger">${formatMoney(calculo.totalPagar)}</strong>
                        </div>
                    </div>
                `;
            }
        }

        montoInput.addEventListener('input', actualizarCalculoCuotas);
        cuotasSelect.addEventListener('change', actualizarCalculoCuotas);
    }
});

// ========================================
// EFECTOS VISUALES ADICIONALES
// ========================================

// Efecto de partículas en el fondo (opcional)
function createParticles() {
    const particlesContainer = document.createElement('div');
    particlesContainer.className = 'particles-container';
    particlesContainer.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        pointer-events: none;
        z-index: -1;
        overflow: hidden;
    `;

    for (let i = 0; i < 20; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        particle.style.cssText = `
            position: absolute;
            width: ${Math.random() * 10 + 5}px;
            height: ${Math.random() * 10 + 5}px;
            background: radial-gradient(circle, rgba(255, 159, 178, 0.3), transparent);
            border-radius: 50%;
            left: ${Math.random() * 100}%;
            top: ${Math.random() * 100}%;
            animation: float ${Math.random() * 10 + 10}s infinite ease-in-out;
        `;
        particlesContainer.appendChild(particle);
    }

    document.body.appendChild(particlesContainer);
}

// CSS para animación de partículas
const style = document.createElement('style');
style.textContent = `
    @keyframes float {
        0%, 100% { transform: translate(0, 0) scale(1); }
        25% { transform: translate(10px, -10px) scale(1.1); }
        50% { transform: translate(-10px, 10px) scale(0.9); }
        75% { transform: translate(5px, 5px) scale(1.05); }
    }
    
    .ripple {
        position: absolute;
        border-radius: 50%;
        background-color: rgba(255, 255, 255, 0.5);
        animation: ripple-effect 0.6s ease-out;
        pointer-events: none;
    }
    
    @keyframes ripple-effect {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }
    
    .btn {
        position: relative;
        overflow: hidden;
    }
`;
document.head.appendChild(style);

// Activar partículas solo en la página de inicio
if (window.location.pathname === '/' || window.location.pathname === '/Index') {
    createParticles();
}

// ========================================
// MODO OSCURO (OPCIONAL - PARA FUTURO)
// ========================================

function toggleDarkMode() {
    document.body.classList.toggle('dark-mode');
    const isDark = document.body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDark);
}

// Cargar preferencia de modo oscuro
document.addEventListener('DOMContentLoaded', function () {
    if (localStorage.getItem('darkMode') === 'true') {
        document.body.classList.add('dark-mode');
    }
});

console.log('✨ Mi Plata - Sistema cargado correctamente');