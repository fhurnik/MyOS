"use client"

import { useMemo, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import Link from "next/link"
import { useTranslations } from "next-intl"
import { createRegisterSchema, type RegisterFormValues } from "@/modules/identity/schemas/register.schema"
import { useRegister } from "@/modules/identity/hooks/useRegister"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { Alert, AlertDescription } from "@/shared/components/ui/alert"
import { AlertCircle, Eye, EyeOff } from "lucide-react"

export function RegisterForm() {
  const t = useTranslations("identity.register")
  const { mutate: register, isPending, error } = useRegister()
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)

  const schema = useMemo(
    () => createRegisterSchema({
      firstNameRequired: t("validation.firstNameRequired"),
      lastNameRequired: t("validation.lastNameRequired"),
      emailInvalid: t("validation.emailInvalid"),
      passwordMinLength: t("validation.passwordMinLength"),
      confirmPasswordRequired: t("validation.confirmPasswordRequired"),
      passwordsMustMatch: t("validation.passwordsMustMatch"),
    }),
    [t]
  )

  const {
    register: field,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(schema),
  })

  const onSubmit = (values: RegisterFormValues) =>
    register({
      firstName: values.firstName,
      lastName: values.lastName,
      email: values.email,
      password: values.password,
    })

  const errorMessage =
    error instanceof Error ? error.message : error ? String(error) : null

  return (
    <div className="w-full max-w-sm space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        {errorMessage && (
          <Alert variant="destructive" className="border-destructive/50 bg-destructive/10">
            <AlertCircle />
            <AlertDescription>{errorMessage}</AlertDescription>
          </Alert>
        )}

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="firstName">{t("firstName")}</Label>
            <Input
              id="firstName"
              aria-invalid={!!errors.firstName}
              {...field("firstName")}
            />
            {errors.firstName && (
              <p className="text-sm text-destructive">{errors.firstName.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="lastName">{t("lastName")}</Label>
            <Input
              id="lastName"
              aria-invalid={!!errors.lastName}
              {...field("lastName")}
            />
            {errors.lastName && (
              <p className="text-sm text-destructive">{errors.lastName.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="email">{t("email")}</Label>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            aria-invalid={!!errors.email}
            {...field("email")}
          />
          {errors.email && (
            <p className="text-sm text-destructive">{errors.email.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password">{t("password")}</Label>
          <div className="relative">
            <Input
              id="password"
              type={showPassword ? "text" : "password"}
              autoComplete="new-password"
              className="pr-10"
              aria-invalid={!!errors.password}
              {...field("password")}
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              aria-label={showPassword ? t("hidePassword") : t("showPassword")}
              aria-pressed={showPassword}
              className="absolute inset-y-0 right-0 flex items-center px-3 text-muted-foreground hover:text-foreground"
            >
              {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="confirmPassword">{t("confirmPassword")}</Label>
          <div className="relative">
            <Input
              id="confirmPassword"
              type={showConfirm ? "text" : "password"}
              autoComplete="new-password"
              className="pr-10"
              aria-invalid={!!errors.confirmPassword}
              {...field("confirmPassword")}
            />
            <button
              type="button"
              onClick={() => setShowConfirm((v) => !v)}
              aria-label={showConfirm ? t("hidePassword") : t("showPassword")}
              aria-pressed={showConfirm}
              className="absolute inset-y-0 right-0 flex items-center px-3 text-muted-foreground hover:text-foreground"
            >
              {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
          )}
        </div>

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending ? "..." : t("submit")}
        </Button>
      </form>

      <p className="text-center text-sm text-muted-foreground">
        {t("hasAccount")}{" "}
        <Link href="../login" className="text-primary underline-offset-4 hover:underline">
          {t("loginLink")}
        </Link>
      </p>
    </div>
  )
}
