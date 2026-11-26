// Expandir o menu lateral
var btnExp = document.querySelector('.btn-open');
var menuSide = document.querySelector('.sidebar');
var mainContainer = document.querySelector('.container');

if (btnExp && menuSide) {
    btnExp.addEventListener('click', function () {
        menuSide.classList.toggle('expandir');
        if (mainContainer) {
            mainContainer.classList.toggle('expandir');
        }
    });
}

// =========================================================================
// GERENCIAMENTO GLOBAL DO MODO ESCURO (DARK MODE EM TODO O SISTEMA)
// =========================================================================
window.applySystemTheme = function (isDark) {
    var htmlEl = document.documentElement;
    var bodyEl = document.body;

    if (isDark) {
        htmlEl.classList.add('dark-mode');
        htmlEl.setAttribute('data-bs-theme', 'dark');
        if (bodyEl) bodyEl.classList.add('dark-mode');
    } else {
        htmlEl.classList.remove('dark-mode');
        htmlEl.setAttribute('data-bs-theme', 'light');
        if (bodyEl) bodyEl.classList.remove('dark-mode');
    }

    localStorage.setItem('vs_theme', isDark ? 'dark' : 'light');
    localStorage.setItem('vs_dash_dark', isDark ? '1' : '0');

    // Atualiza botão da Sidebar
    var sidebarIcon = document.getElementById('sidebarThemeIcon');
    var sidebarLabel = document.getElementById('sidebarThemeLabel');
    if (sidebarIcon) sidebarIcon.textContent = isDark ? 'light_mode' : 'dark_mode';
    if (sidebarLabel) sidebarLabel.textContent = isDark ? 'Modo Claro' : 'Modo Escuro';

    // Atualiza botão e wrapper do Dashboard (se estiver na página do Dashboard)
    var dashWrapper = document.getElementById('dashboardWrapper');
    var dashIcon = document.getElementById('iconTheme');
    var dashLabel = document.getElementById('labelTheme');
    if (dashWrapper) {
        if (isDark) dashWrapper.classList.add('vs-dark-mode');
        else dashWrapper.classList.remove('vs-dark-mode');
    }
    if (dashIcon) dashIcon.textContent = isDark ? 'light_mode' : 'dark_mode';
    if (dashLabel) dashLabel.textContent = isDark ? 'Modo Claro' : 'Modo Escuro';

    // Atualiza cores de textos dos gráficos Chart.js ativos
    if (typeof Chart !== 'undefined') {
        Chart.defaults.color = isDark ? '#D1D5DB' : '#4B5563';
        if (Chart.instances) {
            Object.values(Chart.instances).forEach(function (chart) {
                if (chart && typeof chart.update === 'function') {
                    chart.update();
                }
            });
        }
    }
};

window.toggleSystemTheme = function () {
    var isCurrentlyDark = document.documentElement.classList.contains('dark-mode');
    window.applySystemTheme(!isCurrentlyDark);
};

document.addEventListener('DOMContentLoaded', function () {
    var savedTheme = localStorage.getItem('vs_theme');
    var legacyDark = localStorage.getItem('vs_dash_dark');
    var isDark = savedTheme === 'dark' || (!savedTheme && legacyDark === '1');
    window.applySystemTheme(isDark);

    // =====================================================================
    // LOCALIZAÇÃO GLOBAL PT-BR PARA VALIDAÇÕES E MENSAGENS DE FORMULÁRIOS
    // =====================================================================
    var mapaCamposPtBr = {
        'nome': 'Nome',
        'telefone': 'Telefone',
        'endereco': 'Endereço',
        'situacao': 'Situação Social',
        'observacao': 'Observação',
        'descricao': 'Descrição',
        'razao': 'Razão',
        'categoria': 'Categoria',
        'carteira': 'Carteira',
        'saldo': 'Saldo',
        'valor': 'Valor',
        'hora': 'Data / Hora',
        'cpf': 'CPF',
        'email': 'E-mail',
        'senha': 'Senha',
        'senhaatual': 'Senha Atual',
        'senhanova': 'Nova Senha',
        'novasenha': 'Nova Senha',
        'repetesenhanova': 'Confirmação de Senha',
        'confirmarnovasenha': 'Confirmação de Senha',
        'datanascimento': 'Data de Nascimento',
        'anonascimento': 'Ano de Nascimento',
        'idade': 'Idade',
        'sexo': 'Sexo',
        'parentesco': 'Parentesco',
        'idresponsavel': 'Responsável Familiar',
        'cargo': 'Cargo',
        'usuario': 'Usuário',
        'beneficiario': 'Beneficiário',
        'cor': 'Cor',
        'quantidade': 'Quantidade',
        'local': 'Local',
        'data_inicio': 'Data de Início',
        'data_fim': 'Data de Término'
    };

    function obterNomeCampoAmigavel(nomeRaw) {
        if (!nomeRaw) return 'informado';
        var limpo = nomeRaw.trim().split('.').pop();
        var chave = limpo.toLowerCase();
        return mapaCamposPtBr[chave] || limpo;
    }

    function traduzirTextoValidacaoPtBr(texto) {
        if (!texto) return texto;
        var t = texto.trim();

        var mReq = t.match(/^The\s+(.+?)\s+field\s+is\s+required\.?$/i);
        if (mReq) {
            return 'O campo ' + obterNomeCampoAmigavel(mReq[1]) + ' é obrigatório.';
        }

        var mInv = t.match(/^The\s+value\s+'(.*?)'\s+is\s+(?:not\s+valid|invalid)(?:\s+for\s+(.+?))?\.?$/i);
        if (mInv) {
            var val = mInv[1];
            var campo = obterNomeCampoAmigavel(mInv[2]);
            return val ? ("O valor '" + val + "' não é válido para o campo " + campo + ".") : ('O campo ' + campo + ' é obrigatório.');
        }

        var mNum = t.match(/^The\s+field\s+(.+?)\s+must\s+be\s+a\s+number\.?$/i);
        if (mNum) {
            return 'O campo ' + obterNomeCampoAmigavel(mNum[1]) + ' deve ser um número válido.';
        }

        var mEmail = t.match(/^The\s+(.+?)\s+field\s+is\s+not\s+a\s+valid\s+e-?mail\s+address\.?$/i);
        if (mEmail) {
            return 'Informe um endereço de e-mail válido no campo ' + obterNomeCampoAmigavel(mEmail[1]) + '.';
        }

        if (/^This field is required\.?$/i.test(t) || /^A value is required\.?$/i.test(t)) {
            return 'Este campo é obrigatório.';
        }

        return texto;
    }

    // 1. Traduz atributos data-val-* gerados no HTML
    document.querySelectorAll('[data-val="true"]').forEach(function (el) {
        ['data-val-required', 'data-val-number', 'data-val-email', 'data-val-length', 'data-val-maxlength', 'data-val-minlength', 'data-val-range', 'data-val-regex', 'data-val-equalto'].forEach(function (attr) {
            var val = el.getAttribute(attr);
            if (val) {
                var traduzido = traduzirTextoValidacaoPtBr(val);
                if (traduzido !== val) {
                    el.setAttribute(attr, traduzido);
                }
            }
        });
    });

    // 2. Traduz mensagens de validação já renderizadas na tela
    function traduzirSpansValidacao() {
        document.querySelectorAll('.field-validation-error, .validation-summary-errors li, span.text-danger').forEach(function (el) {
            if (el.childNodes.length === 1 && el.childNodes[0].nodeType === 3) {
                var original = el.textContent;
                var novo = traduzirTextoValidacaoPtBr(original);
                if (novo !== original) {
                    el.textContent = novo;
                }
            } else {
                var txt = (el.innerText || el.textContent || '').trim();
                var traduzido = traduzirTextoValidacaoPtBr(txt);
                if (traduzido !== txt) {
                    el.textContent = traduzido;
                }
            }
        });
    }
    traduzirSpansValidacao();

    // 3. Configura mensagens nativas HTML5 e validação de CPF / Telefone em PT-BR
    function cpfEhValidoJs(cpfRaw) {
        var digitos = (cpfRaw || '').replace(/\D/g, '');
        if (digitos.length !== 11) return false;
        if (/^(\d)\1{10}$/.test(digitos)) return false;

        var soma1 = 0;
        for (var i = 0; i < 9; i++) {
            soma1 += parseInt(digitos.charAt(i), 10) * (10 - i);
        }
        var resto1 = (soma1 * 10) % 11;
        if (resto1 === 10) resto1 = 0;
        if (parseInt(digitos.charAt(9), 10) !== resto1) return false;

        var soma2 = 0;
        for (var j = 0; j < 10; j++) {
            soma2 += parseInt(digitos.charAt(j), 10) * (11 - j);
        }
        var resto2 = (soma2 * 10) % 11;
        if (resto2 === 10) resto2 = 0;
        return parseInt(digitos.charAt(10), 10) === resto2;
    }

    function telefoneEhValidoJs(telRaw) {
        var digitos = (telRaw || '').replace(/\D/g, '');
        if (digitos.length !== 10 && digitos.length !== 11) return false;
        if (/^(\d)\1+$/.test(digitos)) return false;
        var ddd = parseInt(digitos.substring(0, 2), 10);
        return ddd >= 11 && ddd <= 99;
    }

    function validarRegrasCustomizadasCampo(campo) {
        var valor = (campo.value || '').trim();
        if (campo.getAttribute('data-val-cpf') === 'true' && valor.length > 0) {
            if (!cpfEhValidoJs(valor)) {
                campo.setCustomValidity('CPF inválido. Verifique os 11 dígitos informados.');
                return;
            }
        }
        if (campo.getAttribute('data-val-telefone') === 'true' && valor.length > 0) {
            if (!telefoneEhValidoJs(valor)) {
                campo.setCustomValidity('Informe um telefone válido completo com DDD (10 ou 11 dígitos).');
                return;
            }
        }
        campo.setCustomValidity('');
    }

    document.querySelectorAll('input, select, textarea').forEach(function (campo) {
        validarRegrasCustomizadasCampo(campo);

        campo.addEventListener('invalid', function () {
            campo.setCustomValidity('');
            validarRegrasCustomizadasCampo(campo);
            if (!campo.validity.valid && !campo.validationMessage) {
                return;
            }
            if (!campo.validity.valid && !campo.validity.customError) {
                var nome = obterNomeCampoAmigavel(campo.getAttribute('name') || campo.id);
                if (campo.validity.valueMissing) {
                    campo.setCustomValidity('O campo ' + nome + ' é obrigatório.');
                } else if (campo.validity.typeMismatch && campo.type === 'email') {
                    campo.setCustomValidity('Por favor, informe um endereço de e-mail válido.');
                } else if (campo.validity.tooShort) {
                    campo.setCustomValidity('O campo ' + nome + ' deve ter no mínimo ' + campo.minLength + ' caracteres.');
                } else if (campo.validity.tooLong) {
                    campo.setCustomValidity('O campo ' + nome + ' deve ter no máximo ' + campo.maxLength + ' caracteres.');
                } else if (campo.validity.rangeUnderflow || campo.validity.rangeOverflow) {
                    campo.setCustomValidity('O valor informado para ' + nome + ' está fora do intervalo permitido.');
                } else if (campo.validity.patternMismatch) {
                    if (campo.getAttribute('data-val-telefone') === 'true') {
                        campo.setCustomValidity('Informe um telefone válido completo com DDD (10 ou 11 dígitos).');
                    } else if (campo.getAttribute('data-val-cpf') === 'true') {
                        campo.setCustomValidity('CPF inválido. Informe os 11 dígitos numéricos.');
                    } else {
                        campo.setCustomValidity('O formato informado no campo ' + nome + ' é inválido.');
                    }
                }
            }
        });
        campo.addEventListener('input', function () {
            validarRegrasCustomizadasCampo(campo);
        });
        campo.addEventListener('change', function () {
            validarRegrasCustomizadasCampo(campo);
        });
    });
});
