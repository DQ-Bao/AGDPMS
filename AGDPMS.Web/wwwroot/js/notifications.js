// Requires signalr client library to be available as a global `window.signalR`
// This version injects the UMD script and ensures `signalR.HubConnectionBuilder` is usable.
window.notificationClient = {
  start: async function (dotnetRef) {
    if (window._notificationHubConnection) return;

    try {
      async function ensureSignalR() {
        // If signalR is already present and shaped correctly, return it
        if (window.signalR && typeof window.signalR.HubConnectionBuilder === 'function') {
          return window.signalR;
        }

        const fallback = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7/dist/browser/signalr.min.js';

        // Inject UMD script that defines window.signalR
        await new Promise((resolve, reject) => {
          // If the script was already injected by another tab/operation, wait briefly
          if (window.signalR && typeof window.signalR.HubConnectionBuilder === 'function') return resolve();

          // avoid adding duplicate script tags
          const existing = Array.from(document.getElementsByTagName('script')).find(s => s.src && s.src.includes('signalr'));
          if (existing) {
            // wait a little for it to initialize
            const start = Date.now();
            const check = () => {
              if (window.signalR && typeof window.signalR.HubConnectionBuilder === 'function') return resolve();
              if (Date.now() - start > 5000) return reject(new Error('signalR script present but HubConnectionBuilder not available'));
              setTimeout(check, 100);
            };
            return check();
          }

          const s = document.createElement('script');
          s.src = fallback;
          s.async = true;
          s.onload = () => resolve();
          s.onerror = () => reject(new Error('Failed to load SignalR script: ' + fallback));
          document.head.appendChild(s);
        });

        if (!window.signalR || typeof window.signalR.HubConnectionBuilder !== 'function') {
          throw new Error('signalR not available or invalid shape after loading UMD script');
        }

        return window.signalR;
      }

      const signalR = await ensureSignalR();

      // create connection using the global signalR that provides HubConnectionBuilder
      const connection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

      connection.on('ReceiveNotification', function (notification) {
        if (dotnetRef) {
          dotnetRef.invokeMethodAsync('ReceiveNotificationFromJs', notification).catch(console.error);
        }
      });

      connection.on('NotificationUpdated', function (notification) {
        if (dotnetRef) {
          dotnetRef.invokeMethodAsync('NotificationUpdatedFromJs', notification).catch(console.error);
        }
      });

      await connection.start();
      window._notificationHubConnection = connection;
    } catch (err) {
      console.error('notificationClient.start error', err);
    }
  },

  stop: async function () {
    if (window._notificationHubConnection) {
      try {
        await window._notificationHubConnection.stop();
      } catch (err) {
        console.error('notificationClient.stop error', err);
      } finally {
        window._notificationHubConnection = null;
      }
    }
  }
};