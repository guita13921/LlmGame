import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Exporting the Vite configuration that powers the React development server.
// This keeps the setup minimal for candidates while allowing modern tooling features.
export default defineConfig({
  plugins: [react()],
  server: {
    // The port is fixed so the backend can reference it during integration tests.
    port: 5173
  }
});
