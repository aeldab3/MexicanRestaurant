document.addEventListener("DOMContentLoaded", function () {
    const checkoutData = document.getElementById('checkout-data');
    const checkoutForm = document.getElementById('checkoutForm');
    const steps = [...document.querySelectorAll(".checkout-step")];
    let currentStep = parseInt(checkoutData.dataset.step) || 0;

    const stripe = Stripe(checkoutData.dataset.stripeKey)
    const elements = stripe.elements();
    let cardElement;
    const paymentSelect = document.getElementById("paymentMethodSelect");
    const stripeForm = document.getElementById('stripePaymentForm');
    const submitBtn = document.getElementById('submitPayment');
    const overlay = document.getElementById('overlay');

    const showStep = i => {
        steps.forEach((step, idx) => step.style.display = idx === i ? "block" : "none");
        if (i === 3 && paymentSelect.value === 'stripe') {
            stripeForm.style.display = 'block';
            if (!cardElement) {
                cardElement = elements.create('card');
                cardElement.mount('#card-element');
            }
        } else {
            stripeForm.style.display = 'none';
        }
    };

    const validateStep = () => {
        if (currentStep === 0) {
            return [...checkoutForm.querySelectorAll('[name^="ShippingAddress."]')].every(input => input.value.trim() !== '');
        }
        if (currentStep === 1) {
            return !!checkoutForm.querySelector('input[name="SelectedDeliveryMethodId"]:checked');
        }
        return true;
    }

    function updateReview() {
        const getVal = name => document.querySelector(`[name="${name}"]`)?.value || "";
        document.getElementById("reviewName").textContent = `${getVal("ShippingAddress.FirstName")} ${getVal("ShippingAddress.LastName")}`;
        document.getElementById("reviewPhone").textContent = getVal("ShippingAddress.PhoneNumber");
        document.getElementById("reviewAddress").textContent = `${getVal("ShippingAddress.Street")}, ${getVal("ShippingAddress.City")}, ${getVal("ShippingAddress.Zipcode")}`;

        const deliveryInputs = checkoutForm.querySelector('input[name="SelectedDeliveryMethodId"]:checked');
        const label = deliveryInputs?.closest('label')?.textContent || "Not selected";
        const deliveryCost = parseFloat(deliveryInputs?.dataset.price || "0");

        let productTotal = parseFloat(checkoutData.dataset.totalAmount.replace(/[^0-9.-]+/g, "")) || 0;
        const productRows = document.querySelectorAll("#step3 table tbody tr");
        if (productRows.length > 0) {
            productTotal = 0;
            productRows.forEach(row => {
                const priceCell = row.querySelector("td:last-child");
                if (priceCell) {
                    const priceText = priceCell.textContent.replace(/[^0-9.-]+/g, "");
                    productTotal += parseFloat(priceText) || 0;
                }
            });
        }

        document.getElementById("reviewDelivery").innerText = label.trim();
        document.getElementById("reviewTotal").textContent = (productTotal + deliveryCost).toLocaleString("en-US", { style: "currency", currency: "USD" });
    };

    document.querySelectorAll(".next-step").forEach(btn => btn.addEventListener("click", () => {
        if (validateStep()) {
            currentStep++;
            showStep(currentStep);
            if (currentStep === 2) updateReview();
        } else {
            showToast('Please fill in all required fields.', 'warning');
        }
    }));

    document.querySelectorAll(".prev-step").forEach(btn => btn.addEventListener("click", () => {
        if (currentStep > 0) {
            currentStep--;
            showStep(currentStep);
        }
    }));

    paymentSelect.addEventListener("change", () => showStep(currentStep));
    submitBtn.addEventListener("click", async e => {
        e.preventDefault();
        if (!validateStep()) {
            showToast("Please complete required info.", "error");
            currentStep = 0;
            showStep(currentStep);
            return;
        }

        overlay.style.display = 'flex';
        submitBtn.disabled = true;

        const formData = new FormData(checkoutForm);
        const result = await fetch('/Order/Checkout', { method: 'POST', body: formData });
        const data = await result.json();

        overlay.style.display = 'none';
        submitBtn.disabled = false;

        if (!data.isSuccess) {
            showToast(data.message || "Payment failed.", "error");
            if (data.errors) {
                console.log("Errors:", data.errors);
            }
            return;
        }

        if (paymentSelect.value === "stripe") {
            if (!cardElement) {
                showToast("Payment element is not loaded. Please try again.", "error");
                return;
            }
            const { error, paymentIntent } = await stripe.confirmCardPayment(data.clientSecret, {
                payment_method: { card: cardElement }
            });
            if (error) {
                showToast(error.message, "error");
            } else if (paymentIntent.status === "succeeded") {
                window.location.href = "/Order/CheckoutSuccess";
            }
        } else {
            window.location.href = "/Order/CheckoutSuccess";
        }
    });
    showStep(currentStep);
});