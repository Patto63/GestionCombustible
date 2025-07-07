import { createContext, useContext, useState, ReactNode } from 'react';
import jwtDecode from 'jwt-decode';

interface JwtPayload {
  email?: string;
  role?: string | string[];
  [key: string]: any;
}

interface AuthState {
  token: string | null;
  roles: string[];
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(
    () => localStorage.getItem('token')
  );
  const [roles, setRoles] = useState<string[]>(() => {
    if (token) return extractRoles(token);
    return [];
  });

  const login = (newToken: string) => {
    localStorage.setItem('token', newToken);
    setToken(newToken);
    setRoles(extractRoles(newToken));
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setRoles([]);
  };

  return (
    <AuthContext.Provider value={{ token, roles, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('AuthContext not found');
  return ctx;
};

function extractRoles(token: string): string[] {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    const r = decoded.role;
    if (Array.isArray(r)) return r;
    if (typeof r === 'string') return [r];
    return [];
  } catch {
    return [];
  }
}
