document.addEventListener('submit', (e) => {
    const form = e.target;
    if (form.id == 'shop-group-form') {
        e.preventDefault();
        console.log('submit prevented');
        const formData = new FormData(form);
        fetch("/api/group", {
            method: 'POST',
            body: formData
        }).then(r => r.json()).then(j => {
            if (j.status == 'OK') {
                // alert('Додано успішно');
                window.location.reload();
            }
            else {
                alert(j.message);
            }
        });
    }
    else if (form.id == 'shop-product-form') {
        e.preventDefault();
        const formData = new FormData(form);
        fetch("/api/product", {
            method: 'POST',
            body: formData
        }).then(r => r.json()).then(j => {
            if (j.status == 'OK') {
                // alert('Додано успішно');
                window.location.reload();
            }
            else {
                alert(j.message);
            }
        });
    }

});


document.addEventListener('DOMContentLoaded', () => {
    const authButton = document.getElementById("auth-button");
    if (authButton) authButton.addEventListener('click', authClick);
    else console.error("auth-button not found");

    const logOutButton = document.getElementById("log-out-button");
    if (logOutButton) logOutButton.addEventListener('click', logOutClick);

    const profileEditButton = document.getElementById("profile-edit");
    if (profileEditButton) profileEditButton.addEventListener('click', profileEditClick);

    const profileDeleteButton = document.getElementById("profile-delete");
    if (profileDeleteButton) profileDeleteButton.addEventListener('click', profileDeleteClick);

    const productFeedbackButton = document.getElementById("product-feedback-button");
    if (productFeedbackButton) productFeedbackButton.addEventListener('click', productFeedbackClick);

    for (const btn of document.querySelectorAll('[data-role="feedback-edit"]')) {
        btn.addEventListener('click', feedbackEditClick)
    }
    for (const btn of document.querySelectorAll('[data-role="feedback-delete"]')) {
        btn.addEventListener('click', feedbackDeleteClick)
    }
    for (const btn of document.querySelectorAll('[data-role="feedback-recovery"]')) {
        btn.addEventListener('click', feedbackRecoveryClick)
    }
    for (const btn of document.querySelectorAll('[data-role="add-to-cart"]')) {
        btn.addEventListener('click', addToCartClick)
    }
});

function addToCartClick(e) {
    const btn = e.target.closest('[data-role="add-to-cart"]')
    const userId = btn.getAttribute("data-user-id")
    const productId = btn.getAttribute("data-product-id")

    if (!userId) {
        alert("Треба увійти в систему")
        return
    }
    fetch("/api/cart", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            userId,
            productId,
            count: 1
        })
    }).then(r => r.json()).then(j => {
        if (j.data === "Added") {
            alert(`Товар додано успішно, у кошику ${j.meta.count} шт обраних вами товарів`)
        } else {
            alert("Щось пішло не так")
        }
    })
    console.log(userId, productId)
}
function feedbackDeleteClick(e) {
    const feedbackId = e.target.closest('[data-feedback-id]').getAttribute('data-feedback-id')
    if (confirm("Впевнені, що хочете видалити відгук?")) {
        fetch("/api/feedback?id=" + feedbackId, {
            method: 'DELETE'
        }).then(r => r.json()).then(j => {
            if (j.data === "Deleted") {
                window.location.reload()
            } else {
                alert("Щось пішло не так")
            }
        })
        console.log(feedbackId)
    }
}
function feedbackRecoveryClick(e) {
    const feedbackId = e.target.closest('[data-feedback-id]').getAttribute('data-feedback-id')
    if (confirm("Впевнені, що хочете видалити відгук?")) {
        fetch("/api/feedback?id=" + feedbackId, {
            method: 'RECOVERY'
        }).then(r => r.json()).then(j => {
            if (j.data === "Recovered") {
                window.location.reload()
            } else {
                alert("Щось пішло не так")
            }
        })
        console.log(feedbackId)
    }
}
function feedbackEditClick(e) {
    const feedbackId = e.target.closest('[data-feedback-id]').getAttribute('data-feedback-id')
    let text = document.querySelector(`[data-feedback-id="${feedbackId}"][data-role="feedback-text"]`).innerText;
    let rate = document.querySelector(`[data-feedback-id="${feedbackId}"][data-role="feedback-rate"]`).getAttribute('data-value');
    document.getElementById("product-feedback-rate").value = rate
    document.getElementById("product-feedback").value = text
    document.getElementById("product-feedback-title").innerHTML = '<button onclick="productFeedbackCancelEdit()" class="btn btn-danger"><i class="bi bi-x-lg"></i></button> Редагувати відгук:'
    document.getElementById("product-feedback-button").textContent = "Редагувати"
    document.getElementById("product-feedback-button").setAttribute('data-edit-id', feedbackId)
}
function productFeedbackCancelEdit() {
    document.getElementById("product-feedback-rate").value = 5
    document.getElementById("product-feedback").value = ""
    document.getElementById("product-feedback-title").innerHTML = 'Додати відгук:'
    document.getElementById("product-feedback-button").textContent = "Надіслати"
    document.getElementById("data-edit-id").setAttribute('product-feedback-button', feedbackId)
    console.log("Edit canceled")
}
function productFeedbackClick(e) {
    const textarea = document.getElementById("product-feedback")
    const userId = textarea.getAttribute("data-user-id")
    const productId = textarea.getAttribute("data-product-id")
    const timeStamp = textarea.getAttribute("data-timestamp")
    const rate = document.getElementById("product-feedback-rate").value
    var text = textarea.value.trim()
    const editId = e.target.closest('button').getAttribute('data-edit-id')
    if (editId) {
        fetch("/api/feedback", {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                editId,
                text,
                rate
            })
        }).then(r => r.json()).then(j => {
            if (j.data === "Updated") {
                window.location.reload()
            } else {
                alert("Щось пішло не так")
            }
        })
    } else {
        fetch("/api/feedback", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId,
                productId,
                text,
                timeStamp,
                rate
            })
        }).then(r => r.json()).then(j => {
            if (j.data === "Created") {
                window.location.reload()
            } else {
                alert("Щось пішло не так")
            }
        })
    }
}

function profileDeleteClick(e) {
    if (confirm("Підтверджуєте видалення облікового запису?")) {
        fetch(
            "/api/auth", {
            method: "UNLINK",
        })
        .then(r => r.json())
        .then(j => {
            if (j.status == "OK") {
                window.location = "/";
            }
            else {
                alert(j.message);
            }
        });
    }
}

function profileEditClick(e) {
    const btn = e.target;
    const isEditFinish = btn.classList.contains('bi-check2-square');  // <i class="bi bi-check2-square"></i>

    if (isEditFinish) {
        btn.classList.remove('bi-check2-square');
        btn.classList.add('bi-pencil-square');
    }
    else {
        btn.classList.add('bi-check2-square');
        btn.classList.remove('bi-pencil-square');
    }

    let changes = {};
    for (let elem of document.querySelectorAll('[profile-editable]')) {
        if (isEditFinish) {  // завершення редагування
            elem.removeAttribute('contenteditable');
            // треба визначити чи змінювався елемент
            if (elem.initialText != elem.innerText) {  // зміни є
                // дізнаємось назву поля, за яке він відповідає
                const fieldName = elem.getAttribute('profile-editable');
                // console.log(fieldName + ' -> ' + elem.innerText);
                changes[fieldName] = elem.innerText;
            }
        }
        else {  // початок редагування
            // переводимо елементи в режим редагування
            elem.setAttribute('contenteditable', 'true');
            // зберігаємо значення, що було на початок редагування
            elem.initialText = elem.innerText;  // initalText - нове поле, яке
            // JS дозволяє створювати під час роботи (ми самі його придумали)
        }        
    }
    if (isEditFinish) {
        if (Object.keys(changes).length > 0) {
            console.log(changes);
            fetch("/api/auth", {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(changes)
            }).then(r => r.json())
                .then(j => {
                    if (j.status == "OK") {
                        alert(j.message);
                    }
                    else {
                        // а) відновлення початкових даних, оскільки введені
                        // призвели до помилки
                        // for (let elem of document.querySelectorAll('[profile-editable]')) {
                        //     elem.innerText = elem.initialText;
                        // }
                        // б) не вимикаємо режим редагування, точніше включаємо знову
                        for (let elem of document.querySelectorAll('[profile-editable]')) {
                            elem.setAttribute('contenteditable', 'true');
                        }
                        btn.classList.add('bi-check2-square');
                        btn.classList.remove('bi-pencil-square');

                        alert(j.message);
                    }
                });
        }
        // else {
        //     console.log("No changes");
        // }
    }
}

function logOutClick() {
    fetch('/api/auth', {
        method: 'DELETE'
    }).then(r => location.reload());
}

function authClick() {
    const emailInput = document.querySelector('[name="auth-user-email"]');
    if (!emailInput) throw '[name="auth-user-email"] not found';

    const passwordInput = document.querySelector('[name="auth-user-password"]');
    if (!passwordInput) throw '[name="auth-user-password"] not found';

    const errorDiv = document.getElementById("auth-error");
    if (!errorDiv) throw '"auth-error" not found';
    errorDiv.show = err => {
        errorDiv.style.visibility = "visible";
        errorDiv.innerText = err;
    };
    errorDiv.hide = () => {
        errorDiv.style.visibility = "hidden";
        errorDiv.innerText = "";
    };

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (email.length === 0) {
        errorDiv.show("Заповніть E-mail");
        return;
    }
    if (password.length === 0) {
        errorDiv.show("Заповніть пароль");
        return;
    }
    errorDiv.hide();
    console.log(email, password);
    fetch(`/api/auth?email=${email}&password=${password}`, {
        method: 'GET'
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.code != 200) {
            errorDiv.show("Відмова. Перевірьте введені дані.");
        }
        else {
            window.location.reload();
        }
    });
}

/*
    fun() { ***** }

    --- await fun() -------
    ---  *****  -------

    --- fun().then(++++) -------
    --- | --------
        | ***** ++++ (C#)

    --- v --------   =============
                  | ***** ++++ (JS)
*/
