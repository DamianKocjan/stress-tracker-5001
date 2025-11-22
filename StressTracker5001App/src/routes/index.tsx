import { AppFooter } from "@/components/app-footer";
import { AppNav } from "@/components/app-nav";
import { BenefitsSection } from "@/components/benefits-section";
import { FeaturesSection } from "@/components/features-section";
import { HeroSection } from "@/components/hero-section";
import { HowItWorksSection } from "@/components/how-it-works-section";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: IndexComponent,
});

function IndexComponent() {
  return (
    <div className="min-h-screen scroll-smooth">
      <AppNav />

      <HeroSection />

      <FeaturesSection />

      <BenefitsSection />

      <HowItWorksSection />

      <AppFooter />
    </div>
  );
}
