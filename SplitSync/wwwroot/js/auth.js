const forms = document.querySelector(".forms");
const pwShowHide = document.querySelectorAll(".eye-icon");
const links = document.querySelectorAll(".link");

// Password show/hide
pwShowHide.forEach(eyeIcon => {
    eyeIcon.addEventListener("click", () => {
        const field = eyeIcon.closest(".field");
        if (!field) return;
        
        const passwordInput = field.querySelector("input");
        if (!passwordInput) return;

        if (passwordInput.type === "password") {
            passwordInput.type = "text";
            eyeIcon.classList.replace("bx-hide", "bx-show");
        } else {
            passwordInput.type = "password";
            eyeIcon.classList.replace("bx-show", "bx-hide");
        }
    });
});


