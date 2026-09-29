/**
 * Public build configuration. Frontend environment values are NOT secret:
 * never put passwords, JWT keys or connection strings here.
 *
 * There is no hosted API: the static UI deployed on GitHub Pages and the
 * local dev server both point at the API running on the developer's machine
 * (http://localhost:5163). On the deployed demo every data request therefore
 * fails gracefully with "Cannot reach the server. Is the API running?".
 */
export const environment = {
  apiUrl: 'http://localhost:5163',
};
