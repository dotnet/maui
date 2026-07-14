// Handle navigation menu toggle
if (!globalThis.__mauiNavMenuCollapseInitialized) {
    globalThis.__mauiNavMenuCollapseInitialized = true;

    document.addEventListener("click", function(event) {
        const target = event.target;

        if (target instanceof Element && target.closest("#nav-scrollable")) {
            document.querySelector(".navbar-toggler")?.click();
        }
    });
}
