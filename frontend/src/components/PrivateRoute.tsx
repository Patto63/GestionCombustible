import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export const PrivateRoute = ({ roles, children }: { roles?: string[]; children: JSX.Element }) => {
  const { token, roles: userRoles } = useAuth();

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.some(r => userRoles.includes(r))) {
    return <Navigate to="/" replace />;
  }

  return children;
};
