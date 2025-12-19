function openPrintWindow(elementId) {
    const el = document.getElementById(elementId);
    if (!el) {
        console.warn("openPrintWindow: element not found:", elementId);
        return;
    }

    // copy link stylesheets and inline styles from current document
    const cssLinks = Array.from(document.querySelectorAll('link[rel="stylesheet"]'))
        .map(l => `<link rel="stylesheet" href="${l.href}">`)
        .join('\n');
    const inlineStyles = Array.from(document.querySelectorAll('style'))
        .map(s => `<style>${s.innerHTML}</style>`)
        .join('\n');

    // print-specific styles: A4 + only show .print-container
    const printStyles = `<style>
        @page { size: A4; margin: 10mm; }
        html, body { width: 210mm; height: 297mm; margin: 0; padding: 0; }
        /* hide everything by default, then reveal the print container */
        body * { visibility: hidden; }
        .print-container, .print-container * { visibility: visible; }
        /* ensure container occupies a single page area and items break nicely */
        .print-container { box-sizing: border-box; width: 210mm; min-height: 297mm; padding: 10mm; margin: 0 auto; }
        /* prevent unintended page breaks inside a single stamp item */
        .stamp { page-break-inside: avoid; }
    </style>`;

    const content = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
${cssLinks}
${inlineStyles}
${printStyles}
</head>
<body>
  <div class="print-container">${el.innerHTML}</div>
</body>
</html>`;

    const printWindow = window.open('', '_blank', 'width=900,height=700');
    printWindow.document.open();
    printWindow.document.write(content);
    printWindow.document.close();

    // wait for resources to load, then print
    printWindow.onload = function () {
        printWindow.focus();
        printWindow.print();
        // optional: printWindow.close();
    };
}

// Print a specific DOM element (by id) into a new window sized for A4.
// Exposed on window so Blazor Server JSInterop can call it.
window.openPrintWindow = async function (elementId) {
    try {
        // provide a safe default so accidental empty calls still try to print the expected area
        if (!elementId) {
            console.warn("openPrintWindow: elementId is undefined or empty — falling back to 'print-area'");
            elementId = "print-area";
        }

        const el = document.getElementById(elementId);
        if (!el) {
            console.warn("openPrintWindow: element not found:", elementId);
            return;
        }

        const cssLinks = Array.from(document.querySelectorAll('link[rel="stylesheet"]'))
            .map(l => `<link rel="stylesheet" href="${l.href}">`)
            .join('\n');
        const inlineStyles = Array.from(document.querySelectorAll('style'))
            .map(s => `<style>${s.innerHTML}</style>`)
            .join('\n');

        const printStyles = `<style>
            @page { size: A4; margin: 10mm; }
            html, body { width: 210mm; height: 297mm; margin: 0; padding: 0; }
            body * { visibility: hidden; }
            .print-container, .print-container * { visibility: visible; }
            .print-container { box-sizing: border-box; width: 210mm; min-height: 297mm; padding: 10mm; margin: 0 auto; background: white; }
            .stamp-ticket { page-break-inside: avoid; }
        </style>`;

        const cloned = el.cloneNode(true).outerHTML;
        const content = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
${cssLinks}
${inlineStyles}
${printStyles}
</head>
<body>
  <div class="print-container">${cloned}</div>
</body>
</html>`;

        const printWindow = window.open('', '_blank', 'width=900,height=700');
        if (!printWindow) {
            console.warn("openPrintWindow: popup blocked");
            return;
        }

        printWindow.document.open();
        printWindow.document.write(content);
        printWindow.document.close();

        printWindow.onload = function () {
            const imgs = Array.from(printWindow.document.images || []);
            if (imgs.length === 0) {
                printWindow.focus();
                printWindow.print();
                return;
            }
            let loaded = 0;
            imgs.forEach(img => {
                if (img.complete) {
                    loaded++;
                    if (loaded === imgs.length) { printWindow.focus(); printWindow.print(); }
                } else {
                    img.addEventListener('load', () => { loaded++; if (loaded === imgs.length) { printWindow.focus(); printWindow.print(); } });
                    img.addEventListener('error', () => { loaded++; if (loaded === imgs.length) { printWindow.focus(); printWindow.print(); } });
                }
            });
        };
    } catch (err) {
        console.error("openPrintWindow error:", err);
    }
};

// Download file function for Excel exports
window.downloadFile = function(url) {
    try {
        const link = document.createElement('a');
        link.href = url;
        link.download = '';
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (err) {
        console.error("downloadFile error:", err);
    }
};