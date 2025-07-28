document.addEventListener("click", function (e) {
    if (e.target.classList.contains("btn-increase")) {
        const productId = e.target.dataset.productId;
        const stock = parseInt(e.target.dataset.stock);
        const quantitySpan = document.querySelector(`.item-qty[data-product-id='${productId}']`);
        const currentQuantity = parseInt(quantitySpan?.textContent || 0);

        if (stock <= currentQuantity) {
            showToast("❌ Out of Stock.", "warning");
            return;
        }

        fetch("/Order/Increase", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: `productId=${productId}`
        })
        .then(response => {
            if (response.ok) {
                refreshCart();
                showToast("✅ Quantity increased.");
            }
            else showToast("❌ Failed to increase item.", "error");
        });
    }
});

document.addEventListener("click", function (e) {
    if (e.target.classList.contains("btn-decrease")) {
        const productId = e.target.dataset.productId;
        const quantity = parseInt(e.target.dataset.quantity);
        const url = quantity > 1 ? "/Order/Decrease" : "/Order/Remove";

        fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: `productId=${productId}`
        })
        .then(response => {
            if (response.ok) {
                refreshCart();
                if (quantity > 1) {
                    showToast("🔽 Quantity decreased.");
                } else {
                    showToast("🗑️ Product removed from cart.");
                }
            }
            else {
                showToast("❌ Failed to update quantity.", "error");
                return;
            }
        });
    }
});

// 📌 Refresh cart content + cart icon after add/remove/update
async function refreshCart() {
    // update cart content
    const cartRes = await fetch("/Order/CartPartial");
    if (cartRes.ok) {
        const cartContentUpdate = await cartRes.text();
        document.getElementById("cart-container").innerHTML = cartContentUpdate;
    }
    // update cart icon
    const iconRes = await fetch("/Order/CartIconPartial");
    if (iconRes.ok) {
        const cartIconUpdate = await iconRes.text();
        document.getElementById("cart-icon-container").innerHTML = cartIconUpdate;
    }
}