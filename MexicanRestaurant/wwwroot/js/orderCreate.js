document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.btn-add-to-cart').forEach(button => {
        button.addEventListener('click', async () => {
            if (!isAuthenticated()) {
                window.location.href = '/Identity/Account/Login?returnUrl=/Order/Create';
                return;
            }
            const productId = button.dataset.prodId;
            const stock = parseInt(button.dataset.stock);

            const qtyInput = button.closest("form").querySelector('input[name="prodQty"]');
            let quantity = parseInt(qtyInput.value) || 1;

            if (isNaN(quantity) || quantity <= 0) {
                showToast("❌ Please enter a valid quantity.", "warning");
                return;
            }
            if (quantity > stock) {
                showToast("❌ Quantity exceeds available stock.", "warning");
                return;
            }
            // Get the AntiForgeryToken
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            // Send the POST request to add the item to the cart
            const response = await fetch('/Order/AddItem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: `prodId=${productId}&prodQty=${quantity}&__RequestVerificationToken=${token}`
            });

            if (response.ok) {
                const result = await response.json();
                showToast("✅ Product added to cart!");
                await refreshCart();
            }
            else {
                showToast("❌ Failed to add product", "error");
                return;
            }
        });
    });
});
async function refreshCart() {
    const iconResponse = await fetch('/Order/CartIconPartial');
    if (iconResponse.ok) {
        const iconHtml = await iconResponse.text();
        document.getElementById('cart-icon-container').innerHTML = iconHtml;
    }
}

document.addEventListener('click', function (e) {
    if (e.target.classList.contains("btn-out-of-stock")) {
        const name = e.target.dataset.prodName || "This product";
        showToast(`${name} is currently not available.`, "warning");
        return;
    }
});

function isAuthenticated() {
    return document.body.getAttribute("data-authenticated") === "true";
}